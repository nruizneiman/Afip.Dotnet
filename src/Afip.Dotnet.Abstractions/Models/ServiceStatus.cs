using System;

namespace Afip.Dotnet.Abstractions.Models
{
    /// <summary>
    /// Represents the status of an AFIP web service
    /// </summary>
    public class ServiceStatus
    {
        /// <summary>
        /// Application server status
        /// </summary>
        public string AppServer { get; set; } = string.Empty;

        /// <summary>
        /// Database server status
        /// </summary>
        public string DbServer { get; set; } = string.Empty;

        /// <summary>
        /// Authentication server status
        /// </summary>
        public string AuthServer { get; set; } = string.Empty;

        /// <summary>
        /// Service status message
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the service is available
        /// </summary>
        public bool IsAvailable => 
            !string.IsNullOrEmpty(AppServer) && 
            !string.IsNullOrEmpty(DbServer) && 
            !string.IsNullOrEmpty(AuthServer) &&
            AppServer.Equals("OK", StringComparison.OrdinalIgnoreCase) &&
            DbServer.Equals("OK", StringComparison.OrdinalIgnoreCase) &&
            AuthServer.Equals("OK", StringComparison.OrdinalIgnoreCase);
    }
} 