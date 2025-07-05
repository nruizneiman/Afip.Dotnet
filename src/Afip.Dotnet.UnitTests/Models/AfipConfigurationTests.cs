using System;
using System.IO;
using Xunit;
using Afip.Dotnet.Abstractions.Models;

namespace Afip.Dotnet.UnitTests.Models
{
    public class AfipConfigurationTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldSetProperties()
        {
            // Arrange & Act
            var config = new AfipConfiguration
            {
                Environment = AfipEnvironment.Production,
                Cuit = 20123456789,
                CertificatePath = "test.p12",
                CertificatePassword = "password",
                TimeoutSeconds = 60,
                EnableLogging = true
            };

            // Assert
            Assert.Equal(AfipEnvironment.Production, config.Environment);
            Assert.Equal(20123456789, config.Cuit);
            Assert.Equal("test.p12", config.CertificatePath);
            Assert.Equal("password", config.CertificatePassword);
            Assert.Equal(60, config.TimeoutSeconds);
            Assert.True(config.EnableLogging);
        }

        [Fact]
        public void Constructor_WithDefaultValues_ShouldSetDefaults()
        {
            // Arrange & Act
            var config = new AfipConfiguration();

            // Assert
            Assert.Equal(AfipEnvironment.Testing, config.Environment);
            Assert.Equal(0, config.Cuit);
            Assert.Equal(string.Empty, config.CertificatePath);
            Assert.Equal(string.Empty, config.CertificatePassword);
            Assert.Equal(30, config.TimeoutSeconds);
            Assert.False(config.EnableLogging);
        }

        [Fact]
        public void Validate_WithValidConfiguration_ShouldNotThrow()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act & Assert
            var exception = Record.Exception(() => config.Validate());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCuit_ShouldThrowException(long invalidCuit)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Cuit = invalidCuit;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("CUIT must be a valid positive number", exception.Message);
        }

        [Fact]
        public void Validate_WithEmptyCertificatePath_ShouldThrowException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CertificatePath = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("Certificate path is required", exception.Message);
        }

        [Fact]
        public void Validate_WithNonExistentCertificateFile_ShouldThrowException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CertificatePath = "nonexistent.p12";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("Certificate file not found", exception.Message);
        }

        [Fact]
        public void Validate_WithEmptyCertificatePassword_ShouldThrowException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CertificatePassword = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("Certificate password is required", exception.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidTimeout_ShouldThrowException(int invalidTimeout)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.TimeoutSeconds = invalidTimeout;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("Timeout must be greater than 0 seconds", exception.Message);
        }

        [Fact]
        public void Validate_WithTimeoutExceedingMaximum_ShouldThrowException()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.TimeoutSeconds = 301;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("Timeout cannot exceed 300 seconds", exception.Message);
        }

        [Theory]
        [InlineData(1234567890)] // 10 digits
        [InlineData(123456789012)] // 12 digits
        public void Validate_WithInvalidCuitLength_ShouldThrowException(long invalidCuit)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Cuit = invalidCuit;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("CUIT must be exactly 11 digits", exception.Message);
        }

        [Theory]
        [InlineData("cert.crt")]
        [InlineData("cert.pem")]
        [InlineData("cert.cer")]
        public void Validate_WithInvalidCertificateExtension_ShouldThrowException(string invalidPath)
        {
            // Arrange
            var config = new AfipConfiguration
            {
                Cuit = 20123456789,
                CertificatePath = invalidPath,
                CertificatePassword = "password"
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Contains("Certificate file not found", exception.Message);
        }

        [Theory]
        [InlineData(AfipEnvironment.Testing, "https://wsaahomo.afip.gov.ar/ws/services/LoginCms")]
        [InlineData(AfipEnvironment.Production, "https://wsaa.afip.gov.ar/ws/services/LoginCms")]
        public void GetWsaaUrl_ShouldReturnCorrectUrl(AfipEnvironment environment, string expectedUrl)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Environment = environment;

            // Act
            var result = config.GetWsaaUrl();

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GetWsaaUrl_WithCustomUrl_ShouldReturnCustomUrl()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CustomWsaaUrl = "https://custom.wsaa.url";

            // Act
            var result = config.GetWsaaUrl();

            // Assert
            Assert.Equal("https://custom.wsaa.url", result);
        }

        [Theory]
        [InlineData(AfipEnvironment.Testing, "https://wswhomo.afip.gov.ar/wsfev1/service.asmx")]
        [InlineData(AfipEnvironment.Production, "https://servicios1.afip.gov.ar/wsfev1/service.asmx")]
        public void GetWsfev1Url_ShouldReturnCorrectUrl(AfipEnvironment environment, string expectedUrl)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Environment = environment;

            // Act
            var result = config.GetWsfev1Url();

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GetWsfev1Url_WithCustomUrl_ShouldReturnCustomUrl()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CustomWsfev1Url = "https://custom.wsfev1.url";

            // Act
            var result = config.GetWsfev1Url();

            // Assert
            Assert.Equal("https://custom.wsfev1.url", result);
        }

        [Theory]
        [InlineData(AfipEnvironment.Testing, "https://wswhomo.afip.gov.ar/wsfex/service.asmx")]
        [InlineData(AfipEnvironment.Production, "https://servicios1.afip.gov.ar/wsfex/service.asmx")]
        public void GetWsfexUrl_ShouldReturnCorrectUrl(AfipEnvironment environment, string expectedUrl)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Environment = environment;

            // Act
            var result = config.GetWsfexUrl();

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GetWsfexUrl_WithCustomUrl_ShouldReturnCustomUrl()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CustomWsfexUrl = "https://custom.wsfex.url";

            // Act
            var result = config.GetWsfexUrl();

            // Assert
            Assert.Equal("https://custom.wsfex.url", result);
        }

        [Theory]
        [InlineData(AfipEnvironment.Testing, "https://wswhomo.afip.gov.ar/wsmtxca/service.asmx")]
        [InlineData(AfipEnvironment.Production, "https://servicios1.afip.gov.ar/wsmtxca/service.asmx")]
        public void GetWsmtxcaUrl_ShouldReturnCorrectUrl(AfipEnvironment environment, string expectedUrl)
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.Environment = environment;

            // Act
            var result = config.GetWsmtxcaUrl();

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GetWsmtxcaUrl_WithCustomUrl_ShouldReturnCustomUrl()
        {
            // Arrange
            var config = CreateValidConfiguration();
            config.CustomWsmtxcaUrl = "https://custom.wsmtxca.url";

            // Act
            var result = config.GetWsmtxcaUrl();

            // Assert
            Assert.Equal("https://custom.wsmtxca.url", result);
        }

        private AfipConfiguration CreateValidConfiguration()
        {
            // Create a temporary certificate file for testing
            var tempPath = Path.GetTempFileName();
            var certPath = Path.ChangeExtension(tempPath, ".p12");
            File.Move(tempPath, certPath);

            return new AfipConfiguration
            {
                Cuit = 20123456789,
                CertificatePath = certPath,
                CertificatePassword = "password",
                TimeoutSeconds = 30
            };
        }
    }
}