using System;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Services;
using Microsoft.Extensions.Logging;

namespace Afip.Dotnet
{
    /// <summary>
    /// Main client for accessing all AFIP/ARCA web services
    /// </summary>
    public class AfipClient : IAfipClient, IDisposable
    {
        private readonly AfipConfiguration _configuration;
        private readonly ILogger<AfipClient>? _logger;
        private readonly Lazy<IWsaaService> _authenticationService;
        private readonly Lazy<IWsfev1Service> _electronicInvoicingService;
        private readonly Lazy<IWsfexService> _exportInvoicingService;
        private readonly Lazy<IWsmtxcaService> _detailedInvoicingService;
        private readonly Lazy<IAfipParametersService> _parametersService;

        /// <summary>
        /// Initializes a new instance of the AfipClient
        /// </summary>
        /// <param name="configuration">AFIP configuration settings</param>
        /// <param name="logger">Optional logger instance</param>
        public AfipClient(AfipConfiguration configuration, ILogger<AfipClient>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;

            ValidateConfiguration();

            // Initialize services lazily
            _authenticationService = new Lazy<IWsaaService>(() => 
                new WsaaService(_configuration, null));

            _electronicInvoicingService = new Lazy<IWsfev1Service>(() => 
                new Wsfev1Service(_configuration, Authentication, null));

            _exportInvoicingService = new Lazy<IWsfexService>(() => 
                new WsfexService(Authentication, Parameters, null, _configuration));

            _detailedInvoicingService = new Lazy<IWsmtxcaService>(() => 
                new WsmtxcaService(Authentication, Parameters, null, _configuration));

            _parametersService = new Lazy<IAfipParametersService>(() => 
                new AfipParametersService(_configuration, Authentication, null));

            _logger?.LogInformation("AfipClient initialized for CUIT {Cuit} in {Environment} environment", 
                _configuration.Cuit, _configuration.Environment);
        }

        /// <summary>
        /// WSAA authentication service
        /// </summary>
        public IWsaaService Authentication => _authenticationService.Value;

        /// <summary>
        /// WSFEv1 electronic invoicing service
        /// </summary>
        public IWsfev1Service ElectronicInvoicing => _electronicInvoicingService.Value;

        /// <summary>
        /// WSFEX export invoicing service
        /// </summary>
        public IWsfexService ExportInvoicing => _exportInvoicingService.Value;

        /// <summary>
        /// WSMTXCA detailed invoicing service
        /// </summary>
        public IWsmtxcaService DetailedInvoicing => _detailedInvoicingService.Value;

        /// <summary>
        /// AFIP parameters service
        /// </summary>
        public IAfipParametersService Parameters => _parametersService.Value;

        /// <summary>
        /// Creates a new AfipClient instance with the specified configuration
        /// </summary>
        /// <param name="configuration">AFIP configuration settings</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>New AfipClient instance</returns>
        public static AfipClient Create(AfipConfiguration configuration, ILogger<AfipClient>? logger = null)
        {
            return new AfipClient(configuration, logger);
        }

        /// <summary>
        /// Creates a new AfipClient instance for testing environment
        /// </summary>
        /// <param name="cuit">CUIT (tax ID) of the taxpayer</param>
        /// <param name="certificatePath">Path to the PKCS#12 certificate file</param>
        /// <param name="certificatePassword">Password for the certificate file</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>New AfipClient instance configured for testing</returns>
        public static AfipClient CreateForTesting(long cuit, string certificatePath, string certificatePassword, ILogger<AfipClient>? logger = null)
        {
            var configuration = new AfipConfiguration
            {
                Environment = AfipEnvironment.Testing,
                Cuit = cuit,
                CertificatePath = certificatePath,
                CertificatePassword = certificatePassword
            };

            return new AfipClient(configuration, logger);
        }

        /// <summary>
        /// Creates a new AfipClient instance for production environment
        /// </summary>
        /// <param name="cuit">CUIT (tax ID) of the taxpayer</param>
        /// <param name="certificatePath">Path to the PKCS#12 certificate file</param>
        /// <param name="certificatePassword">Password for the certificate file</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>New AfipClient instance configured for production</returns>
        public static AfipClient CreateForProduction(long cuit, string certificatePath, string certificatePassword, ILogger<AfipClient>? logger = null)
        {
            var configuration = new AfipConfiguration
            {
                Environment = AfipEnvironment.Production,
                Cuit = cuit,
                CertificatePath = certificatePath,
                CertificatePassword = certificatePassword
            };

            return new AfipClient(configuration, logger);
        }

        private void ValidateConfiguration()
        {
            if (_configuration.Cuit <= 0)
                throw new ArgumentException("CUIT must be a positive number", nameof(_configuration.Cuit));

            if (string.IsNullOrWhiteSpace(_configuration.CertificatePath))
                throw new ArgumentException("Certificate path is required", nameof(_configuration.CertificatePath));

            if (!System.IO.File.Exists(_configuration.CertificatePath))
                throw new System.IO.FileNotFoundException($"Certificate file not found: {_configuration.CertificatePath}");

            if (_configuration.TimeoutSeconds <= 0)
                throw new ArgumentException("Timeout must be greater than zero", nameof(_configuration.TimeoutSeconds));

            _logger?.LogDebug("Configuration validated successfully");
        }

        /// <summary>
        /// Disposes of resources used by the AfipClient
        /// </summary>
        public void Dispose()
        {
            // Clear any cached authentication tickets
            if (_authenticationService.IsValueCreated)
            {
                Authentication.ClearTicketCache();
            }

            _logger?.LogInformation("AfipClient disposed");
        }
    }

    /// <summary>
    /// Extension methods for AfipClient
    /// </summary>
    public static class AfipClientExtensions
    {
        /// <summary>
        /// Checks if the client is properly configured and can connect to AFIP services
        /// </summary>
        /// <param name="client">The AfipClient instance</param>
        /// <returns>True if the client can connect successfully</returns>
        public static async System.Threading.Tasks.Task<bool> TestConnectionAsync(this AfipClient client)
        {
            try
            {
                var status = await client.ElectronicInvoicing.CheckServiceStatusAsync();
                return status.IsAvailable;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the next available invoice number for the specified point of sale and invoice type
        /// </summary>
        /// <param name="client">The AfipClient instance</param>
        /// <param name="pointOfSale">Point of sale number</param>
        /// <param name="invoiceType">Invoice type code</param>
        /// <returns>Next available invoice number</returns>
        public static async System.Threading.Tasks.Task<long> GetNextInvoiceNumberAsync(this AfipClient client, int pointOfSale, int invoiceType)
        {
            var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(pointOfSale, invoiceType);
            return lastNumber + 1;
        }
    }
}