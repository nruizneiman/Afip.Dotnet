using System;

namespace Afip.Dotnet.Abstractions.Models
{
    /// <summary>
    /// Represents an AFIP/ARCA authentication ticket obtained from WSAA
    /// </summary>
    public class AfipAuthTicket
    {
        /// <summary>
        /// The authentication token
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// The authentication signature
        /// </summary>
        public string Sign { get; set; } = string.Empty;
        
        /// <summary>
        /// When the ticket was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        
        /// <summary>
        /// When the ticket expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// The web service for which this ticket is valid
        /// </summary>
        public string Service { get; set; } = string.Empty;
        
        /// <summary>
        /// Checks if the ticket is still valid (not expired)
        /// </summary>
        public bool IsValid => DateTime.UtcNow < ExpiresAt;
        
        /// <summary>
        /// Checks if the ticket will expire within the specified minutes
        /// </summary>
        /// <param name="minutesBeforeExpiration">Minutes before expiration to check</param>
        /// <returns>True if ticket expires within specified minutes</returns>
        public bool WillExpireSoon(int minutesBeforeExpiration = 10)
        {
            return DateTime.UtcNow.AddMinutes(minutesBeforeExpiration) >= ExpiresAt;
        }
    }
}