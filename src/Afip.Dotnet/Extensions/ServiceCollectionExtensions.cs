using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Services;
using System;

namespace Afip.Dotnet.Extensions
{
    /// <summary>
    /// Extension methods for registering AFIP services in the dependency injection container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds AFIP services to the service collection with the specified configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The AFIP configuration</param>
        /// <param name="enableCaching">Whether to enable caching (default: true)</param>
        /// <param name="enableConnectionPooling">Whether to enable connection pooling (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAfipServices(
            this IServiceCollection services, 
            AfipConfiguration configuration,
            bool enableCaching = true,
            bool enableConnectionPooling = true)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Validate configuration
            configuration.Validate();

            // Register configuration
            services.AddSingleton(configuration);

            // Add memory caching if enabled
            if (enableCaching)
            {
                services.AddMemoryCache();
                services.AddScoped<IAfipCacheService, AfipMemoryCacheService>();
            }

            // Add HTTP client factory and connection pooling if enabled
            if (enableConnectionPooling)
            {
                services.AddHttpClient();
                services.AddScoped<IAfipConnectionPool, AfipConnectionPool>();
            }

            // Register core services
            services.AddScoped<IWsaaService, WsaaService>();
            services.AddScoped<IWsfev1Service, Wsfev1Service>();
            services.AddScoped<IAfipParametersService, AfipParametersService>();
            services.AddScoped<IAfipClient, AfipClient>();

            return services;
        }

        /// <summary>
        /// Adds AFIP services to the service collection with configuration from options
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure AFIP options</param>
        /// <param name="enableCaching">Whether to enable caching (default: true)</param>
        /// <param name="enableConnectionPooling">Whether to enable connection pooling (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAfipServices(
            this IServiceCollection services, 
            Action<AfipConfiguration> configureOptions,
            bool enableCaching = true,
            bool enableConnectionPooling = true)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var configuration = new AfipConfiguration();
            configureOptions(configuration);

            return services.AddAfipServices(configuration, enableCaching, enableConnectionPooling);
        }

        /// <summary>
        /// Adds AFIP services for testing environment with simplified configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="cuit">The CUIT number</param>
        /// <param name="certificatePath">Path to the certificate file</param>
        /// <param name="certificatePassword">Certificate password</param>
        /// <param name="enableCaching">Whether to enable caching (default: true)</param>
        /// <param name="enableConnectionPooling">Whether to enable connection pooling (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAfipServicesForTesting(
            this IServiceCollection services,
            long cuit,
            string certificatePath,
            string certificatePassword,
            bool enableCaching = true,
            bool enableConnectionPooling = true)
        {
            return services.AddAfipServices(config =>
            {
                config.Environment = AfipEnvironment.Testing;
                config.Cuit = cuit;
                config.CertificatePath = certificatePath;
                config.CertificatePassword = certificatePassword;
                config.EnableLogging = true;
            }, enableCaching, enableConnectionPooling);
        }

        /// <summary>
        /// Adds AFIP services for production environment with simplified configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="cuit">The CUIT number</param>
        /// <param name="certificatePath">Path to the certificate file</param>
        /// <param name="certificatePassword">Certificate password</param>
        /// <param name="enableCaching">Whether to enable caching (default: true)</param>
        /// <param name="enableConnectionPooling">Whether to enable connection pooling (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAfipServicesForProduction(
            this IServiceCollection services,
            long cuit,
            string certificatePath,
            string certificatePassword,
            bool enableCaching = true,
            bool enableConnectionPooling = true)
        {
            return services.AddAfipServices(config =>
            {
                config.Environment = AfipEnvironment.Production;
                config.Cuit = cuit;
                config.CertificatePath = certificatePath;
                config.CertificatePassword = certificatePassword;
                config.EnableLogging = true;
            }, enableCaching, enableConnectionPooling);
        }

        /// <summary>
        /// Adds AFIP services with performance optimizations enabled
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The AFIP configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAfipServicesOptimized(
            this IServiceCollection services,
            AfipConfiguration configuration)
        {
            return services.AddAfipServices(configuration, enableCaching: true, enableConnectionPooling: true);
        }

        /// <summary>
        /// Adds AFIP services with minimal dependencies (no caching or connection pooling)
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The AFIP configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAfipServicesMinimal(
            this IServiceCollection services,
            AfipConfiguration configuration)
        {
            return services.AddAfipServices(configuration, enableCaching: false, enableConnectionPooling: false);
        }
    }
}