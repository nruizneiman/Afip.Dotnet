using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Interface for managing HTTP connections to AFIP services with pooling
    /// </summary>
    public interface IAfipConnectionPool : IDisposable
    {
        /// <summary>
        /// Gets an HTTP client for the specified service
        /// </summary>
        /// <param name="serviceName">Name of the AFIP service (wsaa, wsfev1, etc.)</param>
        /// <param name="baseUrl">Base URL for the service</param>
        /// <returns>HTTP client configured for the service</returns>
        HttpClient GetClient(string serviceName, string baseUrl);

        /// <summary>
        /// Executes an HTTP request with retry logic and connection pooling
        /// </summary>
        /// <param name="serviceName">Name of the AFIP service</param>
        /// <param name="baseUrl">Base URL for the service</param>
        /// <param name="requestFunc">Function to execute the HTTP request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HTTP response message</returns>
        Task<HttpResponseMessage> ExecuteRequestAsync(
            string serviceName, 
            string baseUrl, 
            Func<HttpClient, CancellationToken, Task<HttpResponseMessage>> requestFunc,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets connection pool statistics
        /// </summary>
        /// <returns>Connection pool statistics</returns>
        Task<ConnectionPoolStatistics> GetStatisticsAsync();

        /// <summary>
        /// Clears idle connections from the pool
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ClearIdleConnectionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the health of connections in the pool
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Health check results</returns>
        Task<PoolHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Connection pool statistics
    /// </summary>
    public class ConnectionPoolStatistics
    {
        /// <summary>
        /// Total number of active connections
        /// </summary>
        public int ActiveConnections { get; set; }

        /// <summary>
        /// Total number of idle connections
        /// </summary>
        public int IdleConnections { get; set; }

        /// <summary>
        /// Total number of connections created
        /// </summary>
        public long TotalConnectionsCreated { get; set; }

        /// <summary>
        /// Total number of requests handled
        /// </summary>
        public long TotalRequestsHandled { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// Number of failed requests
        /// </summary>
        public long FailedRequests { get; set; }

        /// <summary>
        /// Number of retried requests
        /// </summary>
        public long RetriedRequests { get; set; }

        /// <summary>
        /// Pool efficiency ratio (reused connections / total requests)
        /// </summary>
        public double PoolEfficiency => TotalRequestsHandled > 0 
            ? 1.0 - ((double)TotalConnectionsCreated / TotalRequestsHandled) 
            : 0.0;
    }

    /// <summary>
    /// Health status of the connection pool
    /// </summary>
    public class PoolHealthStatus
    {
        /// <summary>
        /// Overall health status
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Health check timestamp
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Individual service health statuses
        /// </summary>
        public Dictionary<string, ServiceHealthStatus> ServiceStatuses { get; set; } = new Dictionary<string, ServiceHealthStatus>();

        /// <summary>
        /// Health check duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Any health check errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Health status of an individual service
    /// </summary>
    public class ServiceHealthStatus
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Service health status
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Response time for health check
        /// </summary>
        public TimeSpan ResponseTime { get; set; }

        /// <summary>
        /// Error message if unhealthy
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of active connections for this service
        /// </summary>
        public int ActiveConnections { get; set; }
    }

    /// <summary>
    /// Health status enumeration
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// Service is healthy
        /// </summary>
        Healthy,

        /// <summary>
        /// Service is degraded but functional
        /// </summary>
        Degraded,

        /// <summary>
        /// Service is unhealthy
        /// </summary>
        Unhealthy
    }
}