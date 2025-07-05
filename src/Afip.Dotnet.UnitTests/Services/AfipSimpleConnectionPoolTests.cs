using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Afip.Dotnet.UnitTests.Services
{
    public class AfipSimpleConnectionPoolTests : IDisposable
    {
        private readonly Mock<ILogger<AfipSimpleConnectionPool>> _loggerMock;
        private readonly AfipConfiguration _configuration;
        private AfipSimpleConnectionPool _pool;

        public AfipSimpleConnectionPoolTests()
        {
            _loggerMock = new Mock<ILogger<AfipSimpleConnectionPool>>();
            _configuration = new AfipConfiguration
            {
                Environment = AfipEnvironment.Testing,
                Cuit = 20123456789,
                CertificatePath = "test.p12",
                CertificatePassword = "password",
                TimeoutSeconds = 30
            };
            _pool = new AfipSimpleConnectionPool(_loggerMock.Object, _configuration);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_pool);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AfipSimpleConnectionPool(_loggerMock.Object, null!));
        }

        [Fact]
        public void GetClient_WithValidParameters_ShouldReturnHttpClient()
        {
            // Arrange
            var serviceName = "test-service";
            var baseUrl = "https://test.example.com";

            // Act
            var client = _pool.GetClient(serviceName, baseUrl);

            // Assert
            Assert.NotNull(client);
            Assert.IsType<HttpClient>(client);
        }

        [Theory]
        [InlineData("", "https://test.example.com")]
        [InlineData(null, "https://test.example.com")]
        [InlineData("test-service", "")]
        [InlineData("test-service", null)]
        public void GetClient_WithInvalidParameters_ShouldThrowArgumentException(string serviceName, string baseUrl)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pool.GetClient(serviceName, baseUrl));
        }

        [Fact]
        public void GetClient_WithSameParameters_ShouldReturnSameClient()
        {
            // Arrange
            var serviceName = "test-service";
            var baseUrl = "https://test.example.com";

            // Act
            var client1 = _pool.GetClient(serviceName, baseUrl);
            var client2 = _pool.GetClient(serviceName, baseUrl);

            // Assert
            Assert.Same(client1, client2);
        }

        [Fact]
        public void GetClient_WithDifferentParameters_ShouldReturnDifferentClients()
        {
            // Arrange
            var serviceName1 = "test-service-1";
            var serviceName2 = "test-service-2";
            var baseUrl = "https://test.example.com";

            // Act
            var client1 = _pool.GetClient(serviceName1, baseUrl);
            var client2 = _pool.GetClient(serviceName2, baseUrl);

            // Assert
            Assert.NotSame(client1, client2);
        }

        [Fact]
        public async Task ExecuteRequestAsync_WithSuccessfulRequest_ShouldReturnResponse()
        {
            // Arrange
            var serviceName = "test-service";
            var baseUrl = "https://httpbin.org";
            var requestFunc = new Func<HttpClient, CancellationToken, Task<HttpResponseMessage>>(
                async (client, token) => await client.GetAsync("https://httpbin.org/get", token));

            // Act
            var response = await _pool.ExecuteRequestAsync(serviceName, baseUrl, requestFunc);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ExecuteRequestAsync_WithNullRequestFunc_ShouldThrowArgumentNullException()
        {
            // Arrange
            var serviceName = "test-service";
            var baseUrl = "https://test.example.com";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _pool.ExecuteRequestAsync(serviceName, baseUrl, null!, CancellationToken.None));
        }

        [Theory]
        [InlineData("", "https://test.example.com")]
        [InlineData(null, "https://test.example.com")]
        public async Task ExecuteRequestAsync_WithInvalidServiceName_ShouldThrowArgumentException(string serviceName, string baseUrl)
        {
            // Arrange
            var requestFunc = new Func<HttpClient, CancellationToken, Task<HttpResponseMessage>>(
                async (client, token) => await client.GetAsync("https://test.example.com", token));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _pool.ExecuteRequestAsync(serviceName, baseUrl, requestFunc));
        }

        [Fact]
        public async Task GetStatisticsAsync_ShouldReturnValidStatistics()
        {
            // Arrange
            var serviceName = "test-service";
            var baseUrl = "https://test.example.com";
            _pool.GetClient(serviceName, baseUrl);

            // Act
            var stats = await _pool.GetStatisticsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.True(stats.ActiveConnections >= 0);
            Assert.True(stats.TotalConnectionsCreated >= 0);
            Assert.True(stats.TotalRequestsHandled >= 0);
            Assert.True(stats.AverageResponseTimeMs >= 0);
            Assert.True(stats.FailedRequests >= 0);
            Assert.True(stats.RetriedRequests >= 0);
        }

        [Fact]
        public async Task ClearIdleConnectionsAsync_ShouldNotThrow()
        {
            // Act & Assert
            await _pool.ClearIdleConnectionsAsync();
        }

        [Fact]
        public async Task CheckHealthAsync_ShouldReturnHealthStatus()
        {
            // Act
            var healthStatus = await _pool.CheckHealthAsync();

            // Assert
            Assert.NotNull(healthStatus);
            Assert.Equal(HealthStatus.Healthy, healthStatus.Status);
            Assert.NotNull(healthStatus.ServiceStatuses);
            Assert.NotNull(healthStatus.Errors);
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => _pool.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
        {
            // Act & Assert
            _pool.Dispose();
            var exception = Record.Exception(() => _pool.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void GetClient_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            _pool.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => _pool.GetClient("test", "https://test.com"));
        }

        [Fact]
        public async Task ExecuteRequestAsync_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            _pool.Dispose();
            var requestFunc = new Func<HttpClient, CancellationToken, Task<HttpResponseMessage>>(
                async (client, token) => await client.GetAsync("https://test.example.com", token));

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _pool.ExecuteRequestAsync("test", "https://test.com", requestFunc));
        }

        [Fact]
        public async Task GetStatisticsAsync_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            _pool.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _pool.GetStatisticsAsync());
        }

        [Fact]
        public async Task ClearIdleConnectionsAsync_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            _pool.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _pool.ClearIdleConnectionsAsync());
        }

        [Fact]
        public async Task CheckHealthAsync_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            _pool.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _pool.CheckHealthAsync());
        }

        public void Dispose()
        {
            _pool?.Dispose();
        }
    }
} 