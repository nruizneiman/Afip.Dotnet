using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Afip.Dotnet.Abstractions.Services;
using Xunit;
using Xunit.Abstractions;

namespace Afip.Dotnet.IntegrationTests
{
    [Collection("AFIP Integration Tests")]
    public class CachingIntegrationTests
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public CachingIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Cache_Parameter_Tables_Effectively()
        {
            // Arrange
            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping test.");
                return;
            }

            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;

            // Clear cache to start fresh
            await cacheService.ClearAsync();
            var initialStats = await cacheService.GetStatisticsAsync();

            // Act - First calls should populate cache
            var invoiceTypes1 = await client.Parameters.GetInvoiceTypesAsync();
            var documentTypes1 = await client.Parameters.GetDocumentTypesAsync();
            var vatRates1 = await client.Parameters.GetVatRatesAsync();

            var afterFirstCallStats = await cacheService.GetStatisticsAsync();

            // Second calls should hit cache
            var invoiceTypes2 = await client.Parameters.GetInvoiceTypesAsync();
            var documentTypes2 = await client.Parameters.GetDocumentTypesAsync();
            var vatRates2 = await client.Parameters.GetVatRatesAsync();

            var finalStats = await cacheService.GetStatisticsAsync();

            // Assert
            invoiceTypes1.Should().BeEquivalentTo(invoiceTypes2);
            documentTypes1.Should().BeEquivalentTo(documentTypes2);
            vatRates1.Should().BeEquivalentTo(vatRates2);

            finalStats.Hits.Should().BeGreaterThan(afterFirstCallStats.Hits);
            finalStats.HitRatio.Should().BeGreaterThan(0);

            _output.WriteLine($"Cache effectiveness test results:");
            _output.WriteLine($"  Initial: {initialStats.Hits} hits, {initialStats.Misses} misses");
            _output.WriteLine($"  After first calls: {afterFirstCallStats.Hits} hits, {afterFirstCallStats.Misses} misses");
            _output.WriteLine($"  Final: {finalStats.Hits} hits, {finalStats.Misses} misses");
            _output.WriteLine($"  Final hit ratio: {finalStats.HitRatio:P}");
        }

        [Fact]
        public async Task Should_Handle_Cache_Expiration_Correctly()
        {
            // Arrange
            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping test.");
                return;
            }

            const string testKey = "test_cache_expiration";
            const string testValue = "test_value";

            // Act - Set item with very short expiration
            await cacheService.SetAsync(testKey, testValue, TimeSpan.FromMilliseconds(100));
            
            // Should exist immediately
            var exists1 = await cacheService.ExistsAsync(testKey);
            var value1 = await cacheService.GetAsync<string>(testKey);

            // Wait for expiration
            await Task.Delay(200);

            // Should not exist after expiration
            var exists2 = await cacheService.ExistsAsync(testKey);
            var value2 = await cacheService.GetAsync<string>(testKey);

            // Assert
            exists1.Should().BeTrue();
            value1.Should().Be(testValue);
            exists2.Should().BeFalse();
            value2.Should().BeNull();

            _output.WriteLine($"Cache expiration test passed:");
            _output.WriteLine($"  Before expiration - Exists: {exists1}, Value: {value1}");
            _output.WriteLine($"  After expiration - Exists: {exists2}, Value: {value2}");
        }

        [Fact]
        public async Task Should_Support_GetOrSet_Pattern()
        {
            // Arrange
            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping test.");
                return;
            }

            const string testKey = "test_get_or_set";
            var factoryCalled = false;

            // Act - First call should execute factory
            var value1 = await cacheService.GetOrSetAsync(testKey, async (ct) =>
            {
                factoryCalled = true;
                await Task.Delay(10, ct); // Simulate some work
                return "factory_created_value";
            }, TimeSpan.FromMinutes(1));

            factoryCalled.Should().BeTrue();
            factoryCalled = false; // Reset

            // Second call should not execute factory (cached)
            var value2 = await cacheService.GetOrSetAsync(testKey, async (ct) =>
            {
                factoryCalled = true;
                await Task.Delay(10, ct);
                return "should_not_be_called";
            }, TimeSpan.FromMinutes(1));

            // Assert
            value1.Should().Be("factory_created_value");
            value2.Should().Be("factory_created_value");
            factoryCalled.Should().BeFalse(); // Factory should not have been called second time

            _output.WriteLine($"GetOrSet pattern test passed:");
            _output.WriteLine($"  Value 1: {value1}");
            _output.WriteLine($"  Value 2: {value2}");
            _output.WriteLine($"  Factory called on second attempt: {factoryCalled}");
        }

        [Fact]
        public async Task Should_Cache_Authentication_Tickets_With_Proper_Expiration()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            var wsaaService = _fixture.ServiceProvider.GetRequiredService<IWsaaService>();

            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping test.");
                return;
            }

            // Clear cache
            await cacheService.ClearAsync();

            // Act - Get authentication ticket
            var ticket1 = await wsaaService.GetValidTicketAsync("wsfe");
            var stats1 = await cacheService.GetStatisticsAsync();

            // Get again immediately - should be cached
            var ticket2 = await wsaaService.GetValidTicketAsync("wsfe");
            var stats2 = await cacheService.GetStatisticsAsync();

            // Assert
            ticket1.Should().NotBeNull();
            ticket2.Should().NotBeNull();
            ticket1.Token.Should().Be(ticket2.Token);
            ticket1.Sign.Should().Be(ticket2.Sign);

            stats2.Hits.Should().BeGreaterThan(stats1.Hits);

            _output.WriteLine($"Authentication ticket caching test:");
            _output.WriteLine($"  Ticket 1 expires: {ticket1.ExpiresAt}");
            _output.WriteLine($"  Ticket 2 expires: {ticket2.ExpiresAt}");
            _output.WriteLine($"  Cache hits increased: {stats2.Hits > stats1.Hits}");
            _output.WriteLine($"  Tokens match: {ticket1.Token == ticket2.Token}");
        }

        [Fact]
        public async Task Should_Handle_Concurrent_Cache_Operations()
        {
            // Arrange
            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping test.");
                return;
            }

            const int concurrentOperations = 10;
            const string baseKey = "concurrent_test_";

            // Act - Perform concurrent cache operations
            var tasks = new Task[concurrentOperations];
            for (int i = 0; i < concurrentOperations; i++)
            {
                var index = i;
                tasks[i] = Task.Run(async () =>
                {
                    var key = $"{baseKey}{index}";
                    var value = $"value_{index}";
                    
                    await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(1));
                    var retrievedValue = await cacheService.GetAsync<string>(key);
                    
                    retrievedValue.Should().Be(value);
                });
            }

            await Task.WhenAll(tasks);

            // Verify all items are in cache
            var finalStats = await cacheService.GetStatisticsAsync();

            // Assert
            finalStats.ItemCount.Should().BeGreaterOrEqualTo(concurrentOperations);

            _output.WriteLine($"Concurrent cache operations test:");
            _output.WriteLine($"  Operations performed: {concurrentOperations}");
            _output.WriteLine($"  Final cache item count: {finalStats.ItemCount}");
            _output.WriteLine($"  Final hit ratio: {finalStats.HitRatio:P}");
        }

        [Fact]
        public async Task Should_Track_Cache_Statistics_Accurately()
        {
            // Arrange
            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping test.");
                return;
            }

            await cacheService.ClearAsync();
            var initialStats = await cacheService.GetStatisticsAsync();

            // Act - Perform known cache operations
            await cacheService.SetAsync("key1", "value1", TimeSpan.FromMinutes(1));
            await cacheService.SetAsync("key2", "value2", TimeSpan.FromMinutes(1));

            // Two hits
            await cacheService.GetAsync<string>("key1");
            await cacheService.GetAsync<string>("key2");

            // Two misses
            await cacheService.GetAsync<string>("nonexistent1");
            await cacheService.GetAsync<string>("nonexistent2");

            var finalStats = await cacheService.GetStatisticsAsync();

            // Assert
            initialStats.Hits.Should().Be(0);
            initialStats.Misses.Should().Be(0);
            initialStats.ItemCount.Should().Be(0);

            finalStats.ItemCount.Should().Be(2);
            finalStats.Hits.Should().BeGreaterOrEqualTo(2);
            finalStats.Misses.Should().BeGreaterOrEqualTo(2);
            finalStats.HitRatio.Should().BeApproximately(0.5, 0.1); // Approximately 50% hit ratio

            _output.WriteLine($"Cache statistics accuracy test:");
            _output.WriteLine($"  Initial stats - Hits: {initialStats.Hits}, Misses: {initialStats.Misses}, Items: {initialStats.ItemCount}");
            _output.WriteLine($"  Final stats - Hits: {finalStats.Hits}, Misses: {finalStats.Misses}, Items: {finalStats.ItemCount}");
            _output.WriteLine($"  Hit ratio: {finalStats.HitRatio:P}");
        }
    }
}