using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.DependencyInjection.Extensions;
using Xunit;
using Xunit.Sdk;

namespace Afip.Dotnet.IntegrationTests
{
    /// <summary>
    /// Test fixture for integration tests that provides configured AFIP client
    /// </summary>
    public class IntegrationTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IAfipClient AfipClient { get; private set; }
        public AfipConfiguration Configuration { get; private set; }
        
        /// <summary>
        /// Indicates if the certificate is available for integration tests
        /// </summary>
        public bool IsCertificateAvailable => !string.IsNullOrEmpty(Configuration.CertificatePath) && File.Exists(Configuration.CertificatePath);
        
        /// <summary>
        /// Indicates if the environment is suitable for testing
        /// </summary>
        public bool IsTestingEnvironment => Configuration.Environment == AfipEnvironment.Testing;

        public IntegrationTestFixture()
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables("AFIP_")
                .Build();

            // Create AFIP configuration
            Configuration = CreateAfipConfiguration(configuration);

            // Only register services if certificate is available
            if (IsCertificateAvailable && IsTestingEnvironment)
            {
                // Build service provider
                var services = new ServiceCollection();

                // Add AFIP services with all optimizations
                services.AddAfipServicesOptimized(Configuration);

                ServiceProvider = services.BuildServiceProvider();
                AfipClient = ServiceProvider.GetRequiredService<IAfipClient>();

                Console.WriteLine($"Integration test fixture initialized for environment: {Configuration.Environment}");
            }
            else
            {
                // Set to null when certificate is not available
                ServiceProvider = null;
                AfipClient = null;
                Console.WriteLine("Integration test fixture initialized without AFIP services (certificate not available)");
            }
        }

        private AfipConfiguration CreateAfipConfiguration(IConfiguration configuration)
        {
            var afipConfig = new AfipConfiguration();

            // Try to load from configuration
            var cuitString = configuration["Cuit"];
            var certPath = configuration["CertificatePath"];
            var certPassword = configuration["CertificatePassword"];
            var environment = configuration["Environment"];

            if (!string.IsNullOrEmpty(cuitString) && long.TryParse(cuitString, out var cuit))
            {
                afipConfig.Cuit = cuit;
            }
            else
            {
                // Fallback to test values - these should be overridden in CI/CD
                afipConfig.Cuit = 20123456789; // This is a test CUIT, replace with real one
            }

            if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
            {
                afipConfig.CertificatePath = certPath;
            }
            else
            {
                // Try to find test certificate in common locations
                var testCertPaths = new[]
                {
                    "certificates/testing.p12",
                    "test-certificates/testing.p12",
                    "../../../certificates/testing.p12",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".afip", "testing.p12")
                };

                foreach (var testPath in testCertPaths)
                {
                    if (File.Exists(testPath))
                    {
                        afipConfig.CertificatePath = testPath;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(afipConfig.CertificatePath))
                {
                    // Don't throw exception - let SkipIfCertificateNotAvailable handle it
                    afipConfig.CertificatePath = null;
                }
            }

            afipConfig.CertificatePassword = certPassword ?? "test-password";

            if (Enum.TryParse<AfipEnvironment>(environment, out var env))
            {
                afipConfig.Environment = env;
            }
            else
            {
                afipConfig.Environment = AfipEnvironment.Testing; // Default to testing
            }

            afipConfig.EnableLogging = true;
            afipConfig.TimeoutSeconds = 60; // Longer timeout for tests

            return afipConfig;
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }
        }
    }

    /// <summary>
    /// Collection definition for integration tests
    /// </summary>
    [CollectionDefinition("AFIP Integration Tests")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
} 