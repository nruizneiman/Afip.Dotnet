namespace Afip.Dotnet.Abstractions.Models
{
    /// <summary>
    /// Configuration settings for AFIP/ARCA SDK
    /// </summary>
    public class AfipConfiguration
    {
        /// <summary>
        /// The environment to use (Testing or Production)
        /// </summary>
        public AfipEnvironment Environment { get; set; } = AfipEnvironment.Testing;
        
        /// <summary>
        /// The CUIT (tax ID) of the taxpayer
        /// </summary>
        public long Cuit { get; set; }
        
        /// <summary>
        /// Path to the PKCS#12 certificate file (.p12 or .pfx)
        /// </summary>
        public string CertificatePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Password for the certificate file
        /// </summary>
        public string CertificatePassword { get; set; } = string.Empty;
        
        /// <summary>
        /// Timeout for web service calls in seconds (default: 30 seconds)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Whether to enable verbose logging for debugging
        /// </summary>
        public bool EnableLogging { get; set; } = false;
        
        /// <summary>
        /// Custom WSAA URL for testing (optional)
        /// </summary>
        public string? CustomWsaaUrl { get; set; }
        
        /// <summary>
        /// Custom WSFEv1 URL for testing (optional)
        /// </summary>
        public string? CustomWsfev1Url { get; set; }
        
        /// <summary>
        /// Gets the WSAA service URL based on environment
        /// </summary>
        public string WsaaUrl => CustomWsaaUrl ?? (Environment == AfipEnvironment.Production 
            ? "https://wsaa.afip.gov.ar/ws/services/LoginCms"
            : "https://wsaahomo.afip.gov.ar/ws/services/LoginCms");
        
        /// <summary>
        /// Gets the WSFEv1 service URL based on environment
        /// </summary>
        public string Wsfev1Url => CustomWsfev1Url ?? (Environment == AfipEnvironment.Production
            ? "https://servicios1.afip.gov.ar/wsfev1/service.asmx"
            : "https://wswhomo.afip.gov.ar/wsfev1/service.asmx");
    }
}