using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models.Invoice;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Interface for WSFEv1 (Electronic Invoicing Version 1) operations
    /// </summary>
    public interface IWsfev1Service
    {
        /// <summary>
        /// Authorizes a single electronic invoice
        /// </summary>
        /// <param name="request">Invoice authorization request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authorization response</returns>
        Task<InvoiceResponse> AuthorizeInvoiceAsync(InvoiceRequest request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Authorizes multiple electronic invoices in a batch
        /// </summary>
        /// <param name="requests">List of invoice authorization requests</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of authorization responses</returns>
        Task<List<InvoiceResponse>> AuthorizeInvoicesAsync(List<InvoiceRequest> requests, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the last authorized invoice number for a specific point of sale and invoice type
        /// </summary>
        /// <param name="pointOfSale">Point of sale number</param>
        /// <param name="invoiceType">Invoice type code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Last invoice number</returns>
        Task<long> GetLastInvoiceNumberAsync(int pointOfSale, int invoiceType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Queries details of a previously authorized invoice
        /// </summary>
        /// <param name="pointOfSale">Point of sale number</param>
        /// <param name="invoiceType">Invoice type code</param>
        /// <param name="invoiceNumber">Invoice number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Invoice details</returns>
        Task<InvoiceResponse> QueryInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if the web service is available and responsive
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Service status information</returns>
        Task<ServiceStatus> CheckServiceStatusAsync(CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Represents the status of web service components
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
        /// Whether all services are operational
        /// </summary>
        public bool IsHealthy => AppServer == "OK" && DbServer == "OK" && AuthServer == "OK";
    }
}