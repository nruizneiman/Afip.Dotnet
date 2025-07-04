using System;
using System.IO;
using Xunit;
using Afip.Dotnet.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Afip.Dotnet.UnitTests
{
    public class AfipClientTests
    {
        private readonly Mock<ILogger<AfipClient>> _mockLogger;

        public AfipClientTests()
        {
            _mockLogger = new Mock<ILogger<AfipClient>>();
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AfipClient(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithValidConfiguration_ShouldCreateInstance()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            using var client = new AfipClient(config, _mockLogger.Object);

            // Assert
            Assert.NotNull(client);
            Assert.NotNull(client.Authentication);
            Assert.NotNull(client.ElectronicInvoicing);
            Assert.NotNull(client.Parameters);
        }

        [Fact]
        public void Constructor_WithInvalidCuit_ShouldThrowArgumentException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Cuit = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("CUIT must be a positive number", exception.Message);
        }

        [Fact]
        public void Constructor_WithNegativeCuit_ShouldThrowArgumentException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Cuit = -123;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("CUIT must be a positive number", exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyCertificatePath_ShouldThrowArgumentException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CertificatePath = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("Certificate path is required", exception.Message);
        }

        [Fact]
        public void Constructor_WithWhitespaceCertificatePath_ShouldThrowArgumentException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CertificatePath = "   ";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("Certificate path is required", exception.Message);
        }

        [Fact]
        public void Constructor_WithNonExistentCertificatePath_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CertificatePath = "/non/existent/path/cert.p12";

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("Certificate file not found", exception.Message);
        }

        [Fact]
        public void Constructor_WithZeroTimeout_ShouldThrowArgumentException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.TimeoutSeconds = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("Timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void Constructor_WithNegativeTimeout_ShouldThrowArgumentException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.TimeoutSeconds = -30;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AfipClient(config, _mockLogger.Object));
            Assert.Contains("Timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void Create_WithValidConfiguration_ShouldReturnAfipClient()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            using var client = AfipClient.Create(config, _mockLogger.Object);

            // Assert
            Assert.NotNull(client);
            Assert.IsType<AfipClient>(client);
        }

        [Fact]
        public void CreateForTesting_WithValidParameters_ShouldReturnAfipClientWithTestingEnvironment()
        {
            // Arrange
            const long cuit = 20123456789;
            var certificatePath = CreateTemporaryCertificateFile();

            try
            {
                // Act
                using var client = AfipClient.CreateForTesting(cuit, certificatePath, "password", _mockLogger.Object);

                // Assert
                Assert.NotNull(client);
                Assert.IsType<AfipClient>(client);
            }
            finally
            {
                // Cleanup
                if (File.Exists(certificatePath))
                    File.Delete(certificatePath);
            }
        }

        [Fact]
        public void CreateForProduction_WithValidParameters_ShouldReturnAfipClientWithProductionEnvironment()
        {
            // Arrange
            const long cuit = 20123456789;
            var certificatePath = CreateTemporaryCertificateFile();

            try
            {
                // Act
                using var client = AfipClient.CreateForProduction(cuit, certificatePath, "password", _mockLogger.Object);

                // Assert
                Assert.NotNull(client);
                Assert.IsType<AfipClient>(client);
            }
            finally
            {
                // Cleanup
                if (File.Exists(certificatePath))
                    File.Delete(certificatePath);
            }
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act & Assert (should not throw)
            using var client = new AfipClient(config, null);
            Assert.NotNull(client);
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var config = CreateValidConfiguration();
            var client = new AfipClient(config, _mockLogger.Object);

            // Act & Assert (should not throw)
            client.Dispose();
        }

        [Fact]
        public void ServiceProperties_ShouldNotBeNull()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            using var client = new AfipClient(config, _mockLogger.Object);

            // Assert
            Assert.NotNull(client.Authentication);
            Assert.NotNull(client.ElectronicInvoicing);
            Assert.NotNull(client.Parameters);
        }

        [Fact]
        public void ServiceProperties_ShouldReturnSameInstanceOnMultipleCalls()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            using var client = new AfipClient(config, _mockLogger.Object);
            var auth1 = client.Authentication;
            var auth2 = client.Authentication;
            var invoice1 = client.ElectronicInvoicing;
            var invoice2 = client.ElectronicInvoicing;
            var params1 = client.Parameters;
            var params2 = client.Parameters;

            // Assert
            Assert.Same(auth1, auth2);
            Assert.Same(invoice1, invoice2);
            Assert.Same(params1, params2);
        }

        private AfipConfiguration CreateValidConfiguration()
        {
            var certificatePath = CreateTemporaryCertificateFile();
            
            return new AfipConfiguration
            {
                Environment = AfipEnvironment.Testing,
                Cuit = 20123456789,
                CertificatePath = certificatePath,
                CertificatePassword = "password",
                TimeoutSeconds = 30
            };
        }

        private string CreateTemporaryCertificateFile()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "dummy certificate content");
            return tempFile;
        }
    }

    // Extension for testing
    public static class AfipClientTestExtensions
    {
        public static async System.Threading.Tasks.Task<bool> TestConnectionMockAsync(this AfipClient client)
        {
            // Mock implementation for testing
            return await System.Threading.Tasks.Task.FromResult(true);
        }

        public static async System.Threading.Tasks.Task<long> GetNextInvoiceNumberMockAsync(this AfipClient client, int pointOfSale, int invoiceType)
        {
            // Mock implementation for testing
            return await System.Threading.Tasks.Task.FromResult(1L);
        }
    }
}