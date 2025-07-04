using System;
using System.IO;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Afip.Dotnet.DependencyInjection.Extensions;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;

namespace Afip.Dotnet.DependencyInjection.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly IServiceCollection _services;

        public ServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void AddAfipServices_WithNullServices_ShouldThrowArgumentNullException()
        {
            // Arrange
            IServiceCollection? services = null;
            var config = CreateValidConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services!.AddAfipServices(config));
        }

        [Fact]
        public void AddAfipServices_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange
            AfipConfiguration? config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _services.AddAfipServices(config!));
        }

        [Fact]
        public void AddAfipServices_WithValidConfiguration_ShouldRegisterAllServices()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServices(config);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<AfipConfiguration>());
            Assert.NotNull(serviceProvider.GetService<IWsaaService>());
            Assert.NotNull(serviceProvider.GetService<IWsfev1Service>());
            Assert.NotNull(serviceProvider.GetService<IAfipParametersService>());
            Assert.NotNull(serviceProvider.GetService<IAfipClient>());
        }

        [Fact]
        public void AddAfipServices_WithConfigureAction_ShouldRegisterAllServices()
        {
            // Arrange & Act
            _services.AddAfipServices(config =>
            {
                config.Environment = AfipEnvironment.Testing;
                config.Cuit = 20123456789;
                config.CertificatePath = CreateTempCertificateFile();
                config.CertificatePassword = "test-password";
            });
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var configuration = serviceProvider.GetService<AfipConfiguration>();
            Assert.NotNull(configuration);
            Assert.Equal(AfipEnvironment.Testing, configuration.Environment);
            Assert.Equal(20123456789, configuration.Cuit);
            Assert.NotNull(serviceProvider.GetService<IAfipClient>());
        }

        [Fact]
        public void AddAfipServices_WithNullConfigureAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            Action<AfipConfiguration>? configureAction = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _services.AddAfipServices(configureAction!));
        }

        [Fact]
        public void AddAfipServicesForTesting_ShouldConfigureTestingEnvironment()
        {
            // Arrange
            var cuit = 20123456789L;
            var certPath = CreateTempCertificateFile();
            var certPassword = "test-password";

            // Act
            _services.AddAfipServicesForTesting(cuit, certPath, certPassword);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var configuration = serviceProvider.GetService<AfipConfiguration>();
            Assert.NotNull(configuration);
            Assert.Equal(AfipEnvironment.Testing, configuration.Environment);
            Assert.Equal(cuit, configuration.Cuit);
            Assert.Equal(certPath, configuration.CertificatePath);
            Assert.Equal(certPassword, configuration.CertificatePassword);
            Assert.True(configuration.EnableLogging);
        }

        [Fact]
        public void AddAfipServicesForProduction_ShouldConfigureProductionEnvironment()
        {
            // Arrange
            var cuit = 20123456789L;
            var certPath = CreateTempCertificateFile();
            var certPassword = "prod-password";

            // Act
            _services.AddAfipServicesForProduction(cuit, certPath, certPassword);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var configuration = serviceProvider.GetService<AfipConfiguration>();
            Assert.NotNull(configuration);
            Assert.Equal(AfipEnvironment.Production, configuration.Environment);
            Assert.Equal(cuit, configuration.Cuit);
            Assert.Equal(certPath, configuration.CertificatePath);
            Assert.Equal(certPassword, configuration.CertificatePassword);
            Assert.True(configuration.EnableLogging);
        }

        [Fact]
        public void AddAfipServices_WithInvalidConfiguration_ShouldThrowOnValidation()
        {
            // Arrange
            var invalidConfig = new AfipConfiguration
            {
                Cuit = 0, // Invalid CUIT
                CertificatePath = "non-existent-file.p12",
                CertificatePassword = "password"
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _services.AddAfipServices(invalidConfig));
        }

        [Fact]
        public void AddAfipServices_ShouldRegisterServicesAsScoped()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServices(config);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert - Services should be scoped (different instances in different scopes)
            using (var scope1 = serviceProvider.CreateScope())
            using (var scope2 = serviceProvider.CreateScope())
            {
                var client1 = scope1.ServiceProvider.GetService<IAfipClient>();
                var client2 = scope2.ServiceProvider.GetService<IAfipClient>();
                
                Assert.NotNull(client1);
                Assert.NotNull(client2);
                Assert.NotSame(client1, client2); // Different instances
            }
        }

        [Fact]
        public void AddAfipServices_ConfigurationShouldBeSingleton()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServices(config);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert - Configuration should be singleton (same instance across scopes)
            using (var scope1 = serviceProvider.CreateScope())
            using (var scope2 = serviceProvider.CreateScope())
            {
                var config1 = scope1.ServiceProvider.GetService<AfipConfiguration>();
                var config2 = scope2.ServiceProvider.GetService<AfipConfiguration>();
                
                Assert.NotNull(config1);
                Assert.NotNull(config2);
                Assert.Same(config1, config2); // Same instance
            }
        }

        [Fact]
        public void AddAfipServices_WithCachingDisabled_ShouldNotRegisterCacheService()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServices(config, enableCaching: false);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            Assert.Null(serviceProvider.GetService<IAfipCacheService>());
        }

        [Fact]
        public void AddAfipServices_WithConnectionPoolingDisabled_ShouldNotRegisterConnectionPool()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServices(config, enableConnectionPooling: false);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            Assert.Null(serviceProvider.GetService<IAfipConnectionPool>());
        }

        [Fact]
        public void AddAfipServicesOptimized_ShouldEnableCachingAndConnectionPooling()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServicesOptimized(config);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<IAfipCacheService>());
            Assert.NotNull(serviceProvider.GetService<IAfipConnectionPool>());
        }

        [Fact]
        public void AddAfipServicesMinimal_ShouldDisableCachingAndConnectionPooling()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            _services.AddAfipServicesMinimal(config);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            Assert.Null(serviceProvider.GetService<IAfipCacheService>());
            Assert.Null(serviceProvider.GetService<IAfipConnectionPool>());
        }

        private static AfipConfiguration CreateValidConfiguration()
        {
            return new AfipConfiguration
            {
                Environment = AfipEnvironment.Testing,
                Cuit = 20123456789,
                CertificatePath = CreateTempCertificateFile(),
                CertificatePassword = "test-password",
                TimeoutSeconds = 30,
                EnableLogging = true
            };
        }

        private static string CreateTempCertificateFile()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "dummy certificate content");
            return tempFile;
        }
    }
} 