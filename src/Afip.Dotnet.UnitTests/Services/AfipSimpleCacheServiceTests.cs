using System;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace Afip.Dotnet.UnitTests.Services
{
    public class AfipSimpleCacheServiceTests : IDisposable
    {
        private readonly Mock<ILogger<AfipSimpleCacheService>> _loggerMock;
        private readonly AfipSimpleCacheService _cache;

        public AfipSimpleCacheServiceTests()
        {
            _loggerMock = new Mock<ILogger<AfipSimpleCacheService>>();
            _cache = new AfipSimpleCacheService(_loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithValidLogger_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_cache);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => new AfipSimpleCacheService(null));
            Assert.Null(exception);
        }

        [Fact]
        public async Task SetAsync_WithValidKeyAndValue_ShouldStoreValue()
        {
            // Act
            await _cache.SetAsync("test-key", "test-value", TimeSpan.FromMinutes(5));

            // Assert
            var result = await _cache.GetAsync<string>("test-key");
            Assert.Equal("test-value", result);
        }

        [Fact]
        public async Task SetAsync_WithNullKey_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cache.SetAsync<string>(null!, "test-value", TimeSpan.FromMinutes(5)));
        }

        [Fact]
        public async Task SetAsync_WithEmptyKey_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cache.SetAsync("", "test-value", TimeSpan.FromMinutes(5)));
        }

        [Fact]
        public async Task SetAsync_WithWhitespaceKey_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cache.SetAsync("   ", "test-value", TimeSpan.FromMinutes(5)));
        }

        [Fact]
        public async Task SetAsync_WithNullValue_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _cache.SetAsync<string>("test-key", null!, TimeSpan.FromMinutes(5)));
        }

        [Fact]
        public async Task SetAsync_WithZeroExpiration_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cache.SetAsync("test-key", "test-value", TimeSpan.Zero));
        }

        [Fact]
        public async Task SetAsync_WithNegativeExpiration_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cache.SetAsync("test-key", "test-value", TimeSpan.FromSeconds(-1)));
        }

        [Fact]
        public async Task SetAsync_WithExistingKey_ShouldOverwriteValue()
        {
            // Arrange
            await _cache.SetAsync("test-key", "old-value", TimeSpan.FromMinutes(5));

            // Act
            await _cache.SetAsync("test-key", "new-value", TimeSpan.FromMinutes(5));

            // Assert
            var result = await _cache.GetAsync<string>("test-key");
            Assert.Equal("new-value", result);
        }

        [Fact]
        public async Task GetAsync_WithNonExistentKey_ShouldReturnNull()
        {
            // Act
            var result = await _cache.GetAsync<string>("non-existent-key");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WithExpiredKey_ShouldReturnNull()
        {
            // Arrange
            await _cache.SetAsync("expired-key", "test-value", TimeSpan.FromMilliseconds(1));
            await Task.Delay(10); // Wait for expiration

            // Act
            var result = await _cache.GetAsync<string>("expired-key");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveAsync_WithExistingKey_ShouldRemoveKey()
        {
            // Arrange
            await _cache.SetAsync("test-key", "test-value", TimeSpan.FromMinutes(5));

            // Act
            await _cache.RemoveAsync("test-key");
            var result = await _cache.GetAsync<string>("test-key");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveAsync_WithNonExistentKey_ShouldNotThrow()
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _cache.RemoveAsync("non-existent-key"));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ClearAsync_ShouldRemoveAllKeys()
        {
            // Arrange
            await _cache.SetAsync("key1", "value1", TimeSpan.FromMinutes(5));
            await _cache.SetAsync("key2", "value2", TimeSpan.FromMinutes(5));

            // Act
            await _cache.ClearAsync();

            // Assert
            var result1 = await _cache.GetAsync<string>("key1");
            var result2 = await _cache.GetAsync<string>("key2");
            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public async Task GetAsync_WithComplexObject_ShouldWorkCorrectly()
        {
            // Arrange
            var testObject = new TestCacheObject { Id = 1, Name = "Test" };
            await _cache.SetAsync("complex-key", testObject, TimeSpan.FromMinutes(5));

            // Act
            var result = await _cache.GetAsync<TestCacheObject>("complex-key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task SetAsync_WithCustomExpiration_ShouldRespectExpiration()
        {
            // Arrange
            await _cache.SetAsync("short-lived", "value", TimeSpan.FromMilliseconds(50));

            // Act & Assert
            var result1 = await _cache.GetAsync<string>("short-lived");
            Assert.Equal("value", result1);

            await Task.Delay(60); // Wait for expiration

            var result2 = await _cache.GetAsync<string>("short-lived");
            Assert.Null(result2);
        }

        [Fact]
        public async Task MultipleOperations_ShouldWorkCorrectly()
        {
            // Arrange & Act
            await _cache.SetAsync("key1", "value1", TimeSpan.FromMinutes(5));
            await _cache.SetAsync("key2", "value2", TimeSpan.FromMinutes(5));
            await _cache.SetAsync("key3", "value3", TimeSpan.FromMinutes(5));

            var result1 = await _cache.GetAsync<string>("key1");
            var result2 = await _cache.GetAsync<string>("key2");
            var result3 = await _cache.GetAsync<string>("key3");

            await _cache.RemoveAsync("key2");

            var result4 = await _cache.GetAsync<string>("key2");

            // Assert
            Assert.Equal("value1", result1);
            Assert.Equal("value2", result2);
            Assert.Equal("value3", result3);
            Assert.Null(result4);
        }

        [Fact]
        public async Task ConcurrentAccess_ShouldWorkCorrectly()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    await _cache.SetAsync($"key{index}", $"value{index}", TimeSpan.FromMinutes(5));
                    var result = await _cache.GetAsync<string>($"key{index}");
                    Assert.Equal($"value{index}", result);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            for (int i = 0; i < 10; i++)
            {
                var result = await _cache.GetAsync<string>($"key{i}");
                Assert.Equal($"value{i}", result);
            }
        }

        private class TestCacheObject
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
} 