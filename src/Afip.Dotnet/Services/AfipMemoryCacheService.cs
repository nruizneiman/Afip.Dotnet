using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Afip.Dotnet.Abstractions.Services;
using System.Collections.Concurrent;
using System.Collections;

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// In-memory cache implementation for AFIP data
    /// </summary>
    public class AfipMemoryCacheService : IAfipCacheService, IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<AfipMemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _keyTimestamps;
        private readonly object _statsLock = new();
        
        private long _hits;
        private long _misses;
        private bool _disposed;

        public AfipMemoryCacheService(IMemoryCache cache, ILogger<AfipMemoryCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyTimestamps = new ConcurrentDictionary<string, DateTime>();
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

            ThrowIfDisposed();

            if (_cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                Interlocked.Increment(ref _hits);
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult<T?>(typedValue);
            }

            Interlocked.Increment(ref _misses);
            _logger.LogDebug("Cache miss for key: {Key}", key);
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

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal
            };

            options.RegisterPostEvictionCallback(OnItemEvicted);

            _cache.Set(key, value, options);
            _keyTimestamps[key] = DateTime.UtcNow;

            _logger.LogDebug("Cached item with key: {Key}, expiration: {Expiration}", key, expiration);
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

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                Priority = CacheItemPriority.Normal
            };

            options.RegisterPostEvictionCallback(OnItemEvicted);

            _cache.Set(key, value, options);
            _keyTimestamps[key] = DateTime.UtcNow;

            _logger.LogDebug("Cached item with key: {Key}, absolute expiration: {AbsoluteExpiration}", key, absoluteExpiration);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

            ThrowIfDisposed();

            _cache.Remove(key);
            _keyTimestamps.TryRemove(key, out _);

            _logger.LogDebug("Removed item with key: {Key}", key);
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
            _logger.LogDebug("Creating new value for key: {Key}", key);
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

            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (_cache is MemoryCache memoryCache)
            {
                // This is a hack to clear MemoryCache as it doesn't have a public Clear method
                var field = typeof(MemoryCache).GetField("_coherentState", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesCollection = coherentState.GetType()
                        .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is IDictionary entries)
                    {
                        entries.Clear();
                    }
                }
            }

            _keyTimestamps.Clear();
            
            // Reset statistics
            lock (_statsLock)
            {
                _hits = 0;
                _misses = 0;
            }

            _logger.LogInformation("Cache cleared");
            return Task.CompletedTask;
        }

        public Task<CacheStatistics> GetStatisticsAsync()
        {
            ThrowIfDisposed();

            lock (_statsLock)
            {
                var stats = new CacheStatistics
                {
                    Hits = _hits,
                    Misses = _misses,
                    ItemCount = _keyTimestamps.Count
                };

                // Try to get memory usage if possible
                if (_cache is MemoryCache memoryCache)
                {
                    try
                    {
                        var field = typeof(MemoryCache).GetField("_size", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field?.GetValue(memoryCache) is long size)
                        {
                            stats.MemoryUsage = size;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not retrieve memory usage statistics");
                    }
                }

                return Task.FromResult(stats);
            }
        }

        private void OnItemEvicted(object key, object? value, EvictionReason reason, object? state)
        {
            if (key is string stringKey)
            {
                _keyTimestamps.TryRemove(stringKey, out _);
                _logger.LogDebug("Item evicted from cache. Key: {Key}, Reason: {Reason}", stringKey, reason);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AfipMemoryCacheService));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _keyTimestamps.Clear();
                _disposed = true;
                _logger.LogDebug("AfipMemoryCacheService disposed");
            }
        }
    }
}