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

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Simple connection pool implementation for AFIP HTTP clients without external dependencies
    /// </summary>
    public class AfipSimpleConnectionPool : IAfipConnectionPool
    {
        private readonly ILogger<AfipSimpleConnectionPool>? _logger;
        private readonly AfipConfiguration _configuration;
        private readonly ConcurrentDictionary<string, HttpClient> _clients;
        private readonly ConcurrentDictionary<string, ServiceStats> _serviceStats;
        private readonly Timer _cleanupTimer;
        
        private long _totalConnectionsCreated;
        private long _totalRequestsHandled;
        private long _failedRequests;
        private long _retriedRequests;
        private bool _disposed;

        public AfipSimpleConnectionPool(
            ILogger<AfipSimpleConnectionPool>? logger,
            AfipConfiguration configuration)
        {
            _logger = logger;
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
                _logger?.LogDebug("Created new HTTP client for service: {ServiceName}, URL: {BaseUrl}", serviceName, baseUrl);
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
                        _logger?.LogDebug("Request successful for service: {ServiceName}, attempt: {Attempt}, duration: {Duration}ms", 
                            serviceName, attempt + 1, stopwatch.ElapsedMilliseconds);
                        return response;
                    }
                    
                    // Handle specific HTTP status codes
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == (HttpStatusCode)429)
                    {
                        if (attempt < maxRetries)
                        {
                            _logger?.LogWarning("Request failed with status {StatusCode} for service: {ServiceName}, retrying in {RetryDelay}ms", 
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
                    _logger?.LogWarning(ex, "HTTP request failed for service: {ServiceName}, attempt: {Attempt}, retrying in {RetryDelay}ms", 
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
                    
                    _logger?.LogError(ex, "Request timeout for service: {ServiceName}, attempt: {Attempt}", serviceName, attempt + 1);
                    
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
                    
                    _logger?.LogError(ex, "Request failed for service: {ServiceName}, attempt: {Attempt}", serviceName, attempt + 1);
                    
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

            // In this simple implementation, we don't track idle connections
            // so this method just returns immediately
            _logger?.LogDebug("ClearIdleConnectionsAsync called - no idle connections to clear");
            return Task.CompletedTask;
        }

        public async Task<PoolHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var healthStatus = new PoolHealthStatus
            {
                Status = HealthStatus.Healthy,
                Timestamp = DateTimeOffset.UtcNow,
                ServiceStatuses = new System.Collections.Generic.Dictionary<string, ServiceHealthStatus>(),
                Duration = TimeSpan.Zero,
                Errors = new System.Collections.Generic.List<string>()
            };

            // Check each service's health
            foreach (var kvp in _serviceStats)
            {
                var serviceName = kvp.Key;
                var stats = kvp.Value;
                
                var serviceHealth = new ServiceHealthStatus
                {
                    ServiceName = serviceName,
                    Status = HealthStatus.Healthy,
                    ResponseTime = TimeSpan.FromMilliseconds(stats.TotalRequests > 0 ? stats.TotalResponseTimeMs / stats.TotalRequests : 0),
                    ActiveConnections = 1 // Not tracked per service, so set to 1
                };

                // Consider service unhealthy if failure rate is too high
                if (stats.TotalRequests > 10 && (double)stats.FailedRequests / stats.TotalRequests > 0.5)
                {
                    serviceHealth.Status = HealthStatus.Unhealthy;
                    healthStatus.Status = HealthStatus.Unhealthy;
                    serviceHealth.ErrorMessage = "High failure rate";
                }

                healthStatus.ServiceStatuses[serviceName] = serviceHealth;
            }

            return healthStatus;
        }

        private HttpClient CreateHttpClient(string serviceName, string baseUrl)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds)
            };

            // Add default headers
            client.DefaultRequestHeaders.Add("User-Agent", "Afip.Dotnet/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/xml, text/xml, */*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            return client;
        }

        private ServiceStats GetOrCreateServiceStats(string serviceName)
        {
            return _serviceStats.GetOrAdd(serviceName, _ => new ServiceStats());
        }

        private void UpdateResponseTimeStats(ServiceStats stats, long responseTimeMs)
        {
            stats.TotalResponseTimeMs += responseTimeMs;
        }

        private void CleanupIdleConnections(object? state)
        {
            try
            {
                // In this simple implementation, we don't actually clean up connections
                // as we want to keep them for reuse. This method is called by the timer
                // but doesn't perform any cleanup.
                _logger?.LogDebug("Cleanup timer triggered - no cleanup needed in simple implementation");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during connection cleanup");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AfipSimpleConnectionPool));
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