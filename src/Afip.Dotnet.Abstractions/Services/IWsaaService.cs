using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Interface for WSAA (Web Service Authentication and Authorization) operations
    /// </summary>
    public interface IWsaaService
    {
        /// <summary>
        /// Authenticates with AFIP and obtains an access ticket for the specified service
        /// </summary>
        /// <param name="serviceName">Name of the web service (e.g., "wsfe", "wsfev1")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication ticket</returns>
        Task<AfipAuthTicket> AuthenticateAsync(string serviceName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a cached ticket if available and valid, otherwise authenticates and returns a new ticket
        /// </summary>
        /// <param name="serviceName">Name of the web service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication ticket</returns>
        Task<AfipAuthTicket> GetValidTicketAsync(string serviceName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Clears any cached authentication tickets
        /// </summary>
        void ClearTicketCache();
    }
}