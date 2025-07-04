using System;
using System.IO;

namespace Afip.Dotnet.Abstractions.Models
{
    /// <summary>
    /// Configuration settings for AFIP web services
    /// </summary>
    public class AfipConfiguration
    {
        /// <summary>
        /// AFIP environment (Testing or Production)
        /// </summary>
        public AfipEnvironment Environment { get; set; } = AfipEnvironment.Testing;

        /// <summary>
        /// Company CUIT number
        /// </summary>
        public long Cuit { get; set; }

        /// <summary>
        /// Path to the X.509 certificate file (.p12/.pfx)
        /// </summary>
        public string CertificatePath { get; set; } = string.Empty;

        /// <summary>
        /// Password for the certificate file
        /// </summary>
        public string CertificatePassword { get; set; } = string.Empty;

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable detailed logging
        /// </summary>
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        /// Custom WSAA URL (for testing or alternative endpoints)
        /// </summary>
        public string? CustomWsaaUrl { get; set; }

        /// <summary>
        /// Custom WSFEv1 URL (for testing or alternative endpoints)
        /// </summary>
        public string? CustomWsfev1Url { get; set; }

        /// <summary>
        /// Validates the configuration and throws an exception if invalid
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
        public void Validate()
        {
            if (Cuit <= 0)
                throw new InvalidOperationException("CUIT must be a valid positive number");

            if (string.IsNullOrWhiteSpace(CertificatePath))
                throw new InvalidOperationException("Certificate path is required");

            if (!File.Exists(CertificatePath))
                throw new InvalidOperationException($"Certificate file not found: {CertificatePath}");

            if (string.IsNullOrWhiteSpace(CertificatePassword))
                throw new InvalidOperationException("Certificate password is required");

            if (TimeoutSeconds <= 0)
                throw new InvalidOperationException("Timeout must be greater than 0 seconds");

            if (TimeoutSeconds > 300)
                throw new InvalidOperationException("Timeout cannot exceed 300 seconds (5 minutes)");

            // Validate CUIT format (should be 11 digits)
            var cuitString = Cuit.ToString();
            if (cuitString.Length != 11)
                throw new InvalidOperationException("CUIT must be exactly 11 digits");

            // Validate certificate file extension
            var extension = Path.GetExtension(CertificatePath).ToLowerInvariant();
            if (extension != ".p12" && extension != ".pfx")
                throw new InvalidOperationException("Certificate file must be in PKCS#12 format (.p12 or .pfx)");
        }

        /// <summary>
        /// Gets the WSAA URL based on environment and custom settings
        /// </summary>
        public string GetWsaaUrl()
        {
            if (!string.IsNullOrWhiteSpace(CustomWsaaUrl))
                return CustomWsaaUrl;

            return Environment == AfipEnvironment.Testing
                ? "https://wsaahomo.afip.gov.ar/ws/services/LoginCms"
                : "https://wsaa.afip.gov.ar/ws/services/LoginCms";
        }

        /// <summary>
        /// Gets the WSFEv1 URL based on environment and custom settings
        /// </summary>
        public string GetWsfev1Url()
        {
            if (!string.IsNullOrWhiteSpace(CustomWsfev1Url))
                return CustomWsfev1Url;

            return Environment == AfipEnvironment.Testing
                ? "https://wswhomo.afip.gov.ar/wsfev1/service.asmx"
                : "https://servicios1.afip.gov.ar/wsfev1/service.asmx";
        }
    }
}