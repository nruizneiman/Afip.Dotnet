using System;
using System.Threading;
using System.Threading.Tasks;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Interface for caching AFIP data to improve performance
    /// </summary>
    public interface IAfipCacheService
    {
        /// <summary>
        /// Gets a cached value by key
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cached value or null if not found</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Sets a value in the cache with expiration
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Sets a value in the cache with absolute expiration
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="absoluteExpiration">Absolute expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or sets a cached value using a factory function
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Factory function to create the value if not cached</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cached or newly created value</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Checks if a key exists in the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if key exists, false otherwise</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all cached items
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <returns>Cache statistics</returns>
        Task<CacheStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// Total number of cache hits
        /// </summary>
        public long Hits { get; set; }

        /// <summary>
        /// Total number of cache misses
        /// </summary>
        public long Misses { get; set; }

        /// <summary>
        /// Current number of items in cache
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Cache hit ratio (0.0 to 1.0)
        /// </summary>
        public double HitRatio => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) : 0.0;

        /// <summary>
        /// Memory usage in bytes (if available)
        /// </summary>
        public long? MemoryUsage { get; set; }
    }
}