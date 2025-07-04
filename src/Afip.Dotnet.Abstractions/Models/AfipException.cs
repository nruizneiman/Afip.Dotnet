using System;

namespace Afip.Dotnet.Abstractions.Models
{
    /// <summary>
    /// Exception thrown when AFIP/ARCA web service operations fail
    /// </summary>
    public class AfipException : Exception
    {
        /// <summary>
        /// AFIP error code
        /// </summary>
        public int? ErrorCode { get; }
        
        /// <summary>
        /// The web service that generated the error
        /// </summary>
        public string? Service { get; }
        
        public AfipException(string message) : base(message)
        {
        }
        
        public AfipException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        public AfipException(string message, int errorCode, string service) : base(message)
        {
            ErrorCode = errorCode;
            Service = service;
        }
        
        public AfipException(string message, int errorCode, string service, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Service = service;
        }
    }
}