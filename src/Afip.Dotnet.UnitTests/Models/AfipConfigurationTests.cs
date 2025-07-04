using System;
using Xunit;
using Afip.Dotnet.Abstractions.Models;

namespace Afip.Dotnet.UnitTests.Models
{
    public class AfipConfigurationTests
    {
        [Fact]
        public void AfipConfiguration_DefaultValues_ShouldBeCorrect()
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
            Assert.Null(config.CustomWsaaUrl);
            Assert.Null(config.CustomWsfev1Url);
        }

        [Theory]
        [InlineData(AfipEnvironment.Testing, "https://wsaahomo.afip.gov.ar/ws/services/LoginCms")]
        [InlineData(AfipEnvironment.Production, "https://wsaa.afip.gov.ar/ws/services/LoginCms")]
        public void GetWsaaUrl_ShouldReturnCorrectUrlBasedOnEnvironment(AfipEnvironment environment, string expectedUrl)
        {
            // Arrange
            var config = new AfipConfiguration { Environment = environment };

            // Act
            var url = config.GetWsaaUrl();

            // Assert
            Assert.Equal(expectedUrl, url);
        }

        [Theory]
        [InlineData(AfipEnvironment.Testing, "https://wswhomo.afip.gov.ar/wsfev1/service.asmx")]
        [InlineData(AfipEnvironment.Production, "https://servicios1.afip.gov.ar/wsfev1/service.asmx")]
        public void GetWsfev1Url_ShouldReturnCorrectUrlBasedOnEnvironment(AfipEnvironment environment, string expectedUrl)
        {
            // Arrange
            var config = new AfipConfiguration { Environment = environment };

            // Act
            var url = config.GetWsfev1Url();

            // Assert
            Assert.Equal(expectedUrl, url);
        }

        [Fact]
        public void GetWsaaUrl_WithCustomUrl_ShouldReturnCustomUrl()
        {
            // Arrange
            const string customUrl = "https://custom.wsaa.url";
            var config = new AfipConfiguration 
            { 
                Environment = AfipEnvironment.Production,
                CustomWsaaUrl = customUrl
            };

            // Act
            var url = config.GetWsaaUrl();

            // Assert
            Assert.Equal(customUrl, url);
        }

        [Fact]
        public void GetWsfev1Url_WithCustomUrl_ShouldReturnCustomUrl()
        {
            // Arrange
            const string customUrl = "https://custom.wsfev1.url";
            var config = new AfipConfiguration 
            { 
                Environment = AfipEnvironment.Production,
                CustomWsfev1Url = customUrl
            };

            // Act
            var url = config.GetWsfev1Url();

            // Assert
            Assert.Equal(customUrl, url);
        }

        [Fact]
        public void AfipConfiguration_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new AfipConfiguration
            {
                Environment = AfipEnvironment.Production,
                Cuit = 20123456789,
                CertificatePath = "/path/to/cert.p12",
                CertificatePassword = "password123",
                TimeoutSeconds = 60,
                EnableLogging = true,
                CustomWsaaUrl = "https://custom.wsaa.url",
                CustomWsfev1Url = "https://custom.wsfev1.url"
            };

            // Act & Assert
            Assert.Equal(AfipEnvironment.Production, config.Environment);
            Assert.Equal(20123456789, config.Cuit);
            Assert.Equal("/path/to/cert.p12", config.CertificatePath);
            Assert.Equal("password123", config.CertificatePassword);
            Assert.Equal(60, config.TimeoutSeconds);
            Assert.True(config.EnableLogging);
            Assert.Equal("https://custom.wsaa.url", config.CustomWsaaUrl);
            Assert.Equal("https://custom.wsfev1.url", config.CustomWsfev1Url);
        }
    }
}