using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Abstractions.Models;
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Count

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Connection pool implementation for AFIP HTTP clients with retry logic and health monitoring
    /// </summary>
    public class AfipConnectionPool : IAfipConnectionPool
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AfipConnectionPool> _logger;
        private readonly AfipConfiguration _configuration;
        private readonly ConcurrentDictionary<string, HttpClient> _clients;
        private readonly ConcurrentDictionary<string, ServiceStats> _serviceStats;
        private readonly Timer _cleanupTimer;
        
        private long _totalConnectionsCreated;
        private long _totalRequestsHandled;
        private long _failedRequests;
        private long _retriedRequests;
        private bool _disposed;

        public AfipConnectionPool(
            IHttpClientFactory httpClientFactory, 
            ILogger<AfipConnectionPool> logger,
            AfipConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _clients = new ConcurrentDictionary<string, HttpClient>();
            _serviceStats = new ConcurrentDictionary<string, ServiceStats>();
            
            // Start cleanup timer to run every 5 minutes
            _cleanupTimer = new Timer(CleanupIdleConnections, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public HttpClient GetClient(string serviceName, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or whitespace", nameof(serviceName));
            
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or whitespace", nameof(baseUrl));

            ThrowIfDisposed();

            var key = $"{serviceName}_{baseUrl}";
            
            return _clients.GetOrAdd(key, _ =>
            {
                var client = CreateHttpClient(serviceName, baseUrl);
                Interlocked.Increment(ref _totalConnectionsCreated);
                _logger.LogDebug("Created new HTTP client for service: {ServiceName}, URL: {BaseUrl}", serviceName, baseUrl);
                return client;
            });
        }

        public async Task<HttpResponseMessage> ExecuteRequestAsync(
            string serviceName, 
            string baseUrl, 
            Func<HttpClient, CancellationToken, Task<HttpResponseMessage>> requestFunc,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or whitespace", nameof(serviceName));
            
            if (requestFunc == null)
                throw new ArgumentNullException(nameof(requestFunc));

            ThrowIfDisposed();

            var client = GetClient(serviceName, baseUrl);
            var stats = GetOrCreateServiceStats(serviceName);
            var stopwatch = Stopwatch.StartNew();
            
            Interlocked.Increment(ref _totalRequestsHandled);
            Interlocked.Increment(ref stats.TotalRequests);

            var maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(1);

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await requestFunc(client, cancellationToken);
                    
                    stopwatch.Stop();
                    UpdateResponseTimeStats(stats, stopwatch.ElapsedMilliseconds);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("Request successful for service: {ServiceName}, attempt: {Attempt}, duration: {Duration}ms", 
                            serviceName, attempt + 1, stopwatch.ElapsedMilliseconds);
                        return response;
                    }
                    
                    // Handle specific HTTP status codes
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        if (attempt < maxRetries)
                        {
                            _logger.LogWarning("Request failed with status {StatusCode} for service: {ServiceName}, retrying in {RetryDelay}ms", 
                                response.StatusCode, serviceName, retryDelay.TotalMilliseconds);
                            
                            response.Dispose();
                            await Task.Delay(retryDelay, cancellationToken);
                            retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
                            Interlocked.Increment(ref _retriedRequests);
                            continue;
                        }
                    }
                    
                    return response;
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, "HTTP request failed for service: {ServiceName}, attempt: {Attempt}, retrying in {RetryDelay}ms", 
                        serviceName, attempt + 1, retryDelay.TotalMilliseconds);
                    
                    await Task.Delay(retryDelay, cancellationToken);
                    retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2);
                    Interlocked.Increment(ref _retriedRequests);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout occurred
                    stopwatch.Stop();
                    Interlocked.Increment(ref _failedRequests);
                    Interlocked.Increment(ref stats.FailedRequests);
                    
                    _logger.LogError(ex, "Request timeout for service: {ServiceName}, attempt: {Attempt}", serviceName, attempt + 1);
                    
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelay, cancellationToken);
                        retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2);
                        Interlocked.Increment(ref _retriedRequests);
                        continue;
                    }
                    
                    throw new AfipException($"Request timeout after {maxRetries + 1} attempts for service: {serviceName}", ex);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Interlocked.Increment(ref _failedRequests);
                    Interlocked.Increment(ref stats.FailedRequests);
                    
                    _logger.LogError(ex, "Request failed for service: {ServiceName}, attempt: {Attempt}", serviceName, attempt + 1);
                    
                    if (attempt == maxRetries)
                    {
                        throw new AfipException($"Request failed after {maxRetries + 1} attempts for service: {serviceName}", ex);
                    }
                }
            }

            throw new AfipException($"All retry attempts exhausted for service: {serviceName}");
        }

        public Task<ConnectionPoolStatistics> GetStatisticsAsync()
        {
            ThrowIfDisposed();

            var totalResponseTime = 0.0;
            var totalRequests = 0L;

            foreach (var stats in _serviceStats.Values)
            {
                totalResponseTime += stats.TotalResponseTimeMs;
                totalRequests += stats.TotalRequests;
            }

            var statistics = new ConnectionPoolStatistics
            {
                ActiveConnections = _clients.Count,
                IdleConnections = 0, // We don't track idle connections separately in this implementation
                TotalConnectionsCreated = _totalConnectionsCreated,
                TotalRequestsHandled = _totalRequestsHandled,
                AverageResponseTimeMs = totalRequests > 0 ? totalResponseTime / totalRequests : 0,
                FailedRequests = _failedRequests,
                RetriedRequests = _retriedRequests
            };

            return Task.FromResult(statistics);
        }

        public Task ClearIdleConnectionsAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            // In this implementation, we don't have explicit idle connection tracking
            // This could be enhanced to track last usage time and dispose unused clients
            _logger.LogDebug("Idle connection cleanup requested");
            return Task.CompletedTask;
        }

        public async Task<PoolHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var healthStatus = new PoolHealthStatus
            {
                Timestamp = DateTimeOffset.UtcNow,
                ServiceStatuses = new Dictionary<string, ServiceHealthStatus>()
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var healthTasks = new List<Task>();

                foreach (var kvp in _serviceStats)
                {
                    var serviceName = kvp.Key;
                    var stats = kvp.Value;

                    healthTasks.Add(CheckServiceHealthAsync(serviceName, stats, healthStatus, cancellationToken));
                }

                await Task.WhenAll(healthTasks);

                // Determine overall health
                var healthyCount = healthStatus.ServiceStatuses.Count(s => s.Value.Status == HealthStatus.Healthy);
                var degradedCount = healthStatus.ServiceStatuses.Count(s => s.Value.Status == HealthStatus.Degraded);
                var unhealthyCount = healthStatus.ServiceStatuses.Count(s => s.Value.Status == HealthStatus.Unhealthy);

                if (unhealthyCount > 0)
                    healthStatus.Status = HealthStatus.Unhealthy;
                else if (degradedCount > 0)
                    healthStatus.Status = HealthStatus.Degraded;
                else
                    healthStatus.Status = HealthStatus.Healthy;
            }
            catch (Exception ex)
            {
                healthStatus.Status = HealthStatus.Unhealthy;
                healthStatus.Errors.Add($"Health check failed: {ex.Message}");
                _logger.LogError(ex, "Health check failed");
            }
            finally
            {
                stopwatch.Stop();
                healthStatus.Duration = stopwatch.Elapsed;
            }

            return healthStatus;
        }

        private async Task CheckServiceHealthAsync(
            string serviceName, 
            ServiceStats stats, 
            PoolHealthStatus healthStatus, 
            CancellationToken cancellationToken)
        {
            var serviceHealth = new ServiceHealthStatus
            {
                ServiceName = serviceName,
                ActiveConnections = 1 // We have one client per service in this implementation
            };

            var healthCheckStopwatch = Stopwatch.StartNew();

            try
            {
                // Simple health check based on error rate and response times
                var errorRate = stats.TotalRequests > 0 ? (double)stats.FailedRequests / stats.TotalRequests : 0;
                var avgResponseTime = stats.TotalRequests > 0 ? stats.TotalResponseTimeMs / stats.TotalRequests : 0;

                if (errorRate > 0.5) // More than 50% error rate
                {
                    serviceHealth.Status = HealthStatus.Unhealthy;
                    serviceHealth.ErrorMessage = $"High error rate: {errorRate:P}";
                }
                else if (errorRate > 0.2 || avgResponseTime > 10000) // More than 20% error rate or avg > 10s
                {
                    serviceHealth.Status = HealthStatus.Degraded;
                    serviceHealth.ErrorMessage = $"Degraded performance: {errorRate:P} error rate, {avgResponseTime:F0}ms avg response time";
                }
                else
                {
                    serviceHealth.Status = HealthStatus.Healthy;
                }
            }
            catch (Exception ex)
            {
                serviceHealth.Status = HealthStatus.Unhealthy;
                serviceHealth.ErrorMessage = ex.Message;
            }
            finally
            {
                healthCheckStopwatch.Stop();
                serviceHealth.ResponseTime = healthCheckStopwatch.Elapsed;
                healthStatus.ServiceStatuses[serviceName] = serviceHealth;
            }
        }

        private HttpClient CreateHttpClient(string serviceName, string baseUrl)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);
            
            // Set common headers
            client.DefaultRequestHeaders.Add("User-Agent", "Afip.Dotnet.SDK/1.0");
            client.DefaultRequestHeaders.Add("Accept", "text/xml, application/soap+xml, application/xml");
            
            return client;
        }

        private ServiceStats GetOrCreateServiceStats(string serviceName)
        {
            return _serviceStats.GetOrAdd(serviceName, _ => new ServiceStats());
        }

        private void UpdateResponseTimeStats(ServiceStats stats, long responseTimeMs)
        {
            lock (stats)
            {
                stats.TotalResponseTimeMs += responseTimeMs;
            }
        }

        private void CleanupIdleConnections(object? state)
        {
            try
            {
                // This is a placeholder for more sophisticated idle connection cleanup
                // In a real implementation, you might track last usage time and dispose unused clients
                _logger.LogDebug("Periodic connection cleanup executed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during connection cleanup");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AfipConnectionPool));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Dispose();
                
                foreach (var client in _clients.Values)
                {
                    client.Dispose();
                }
                
                _clients.Clear();
                _serviceStats.Clear();
                
                _disposed = true;
                _logger.LogDebug("AfipConnectionPool disposed");
            }
        }

        private class ServiceStats
        {
            public long TotalRequests;
            public long FailedRequests;
            public double TotalResponseTimeMs;
        }
    }
}