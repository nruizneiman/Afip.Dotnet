using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Simple in-memory cache implementation for AFIP data without external dependencies
    /// </summary>
    public class AfipSimpleCacheService : IAfipCacheService, IDisposable
    {
        private readonly ILogger<AfipSimpleCacheService>? _logger;
        private readonly ConcurrentDictionary<string, CacheItem> _cache;
        private readonly Timer _cleanupTimer;
        private bool _disposed;
        private long _hits;
        private long _misses;

        public AfipSimpleCacheService(ILogger<AfipSimpleCacheService>? logger = null)
        {
            _logger = logger;
            _cache = new ConcurrentDictionary<string, CacheItem>();
            _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

            ThrowIfDisposed();

            if (_cache.TryGetValue(key, out var item) && item.IsValid && item.Value is T typedValue)
            {
                Interlocked.Increment(ref _hits);
                _logger?.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult<T?>(typedValue);
            }

            Interlocked.Increment(ref _misses);
            _logger?.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (expiration <= TimeSpan.Zero)
                throw new ArgumentException("Expiration must be positive", nameof(expiration));

            ThrowIfDisposed();

            var item = new CacheItem
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(expiration)
            };

            _cache.AddOrUpdate(key, item, (k, v) => item);

            _logger?.LogDebug("Cached item with key: {Key}, expiration: {Expiration}", key, expiration);
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (absoluteExpiration <= DateTimeOffset.UtcNow)
                throw new ArgumentException("Absolute expiration must be in the future", nameof(absoluteExpiration));

            ThrowIfDisposed();

            var item = new CacheItem
            {
                Value = value,
                ExpiresAt = absoluteExpiration.UtcDateTime
            };

            _cache.AddOrUpdate(key, item, (k, v) => item);

            _logger?.LogDebug("Cached item with key: {Key}, absolute expiration: {AbsoluteExpiration}", key, absoluteExpiration);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

            ThrowIfDisposed();

            _cache.TryRemove(key, out _);

            _logger?.LogDebug("Removed item with key: {Key}", key);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
            
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            ThrowIfDisposed();

            // Try to get from cache first
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
                return cached;

            // Create new value
            _logger?.LogDebug("Creating new value for key: {Key}", key);
            var value = await factory(cancellationToken);
            
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }

            return value;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

            ThrowIfDisposed();

            var exists = _cache.TryGetValue(key, out var item) && item.IsValid;
            return Task.FromResult(exists);
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _cache.Clear();
            _logger?.LogInformation("Cache cleared");
            return Task.CompletedTask;
        }

        public Task<CacheStatistics> GetStatisticsAsync()
        {
            ThrowIfDisposed();

            var stats = new CacheStatistics
            {
                ItemCount = _cache.Count,
                Hits = _hits,
                Misses = _misses,
                MemoryUsage = null
            };

            return Task.FromResult(stats);
        }

        private void CleanupExpiredItems(object? state)
        {
            try
            {
                var expiredKeys = _cache
                    .Where(kvp => !kvp.Value.IsValid)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger?.LogDebug("Cleaned up {Count} expired cache items", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during cache cleanup");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AfipSimpleCacheService));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Dispose();
                _cache.Clear();
                _disposed = true;
            }
        }

        private class CacheItem
        {
            public object Value { get; set; } = null!;
            public DateTime ExpiresAt { get; set; }
            public bool IsValid => DateTime.UtcNow < ExpiresAt;
        }
    }
} 