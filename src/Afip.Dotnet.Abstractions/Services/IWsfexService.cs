using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Service for AFIP WSFEX (Electronic Invoicing for Exports) operations
    /// </summary>
    public interface IWsfexService
    {
        /// <summary>
        /// Checks the service status
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Service status information</returns>
        Task<ServiceStatus> CheckServiceStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the last invoice number for a specific point of sale and invoice type
        /// </summary>
        /// <param name="pointOfSale">Point of sale number</param>
        /// <param name="invoiceType">Invoice type (E for exports)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Last invoice number</returns>
        Task<long> GetLastInvoiceNumberAsync(int pointOfSale, int invoiceType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authorizes an export invoice
        /// </summary>
        /// <param name="request">Export invoice request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Export invoice response</returns>
        Task<ExportInvoiceResponse> AuthorizeExportInvoiceAsync(ExportInvoiceRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries an export invoice by its details
        /// </summary>
        /// <param name="pointOfSale">Point of sale number</param>
        /// <param name="invoiceType">Invoice type</param>
        /// <param name="invoiceNumber">Invoice number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Export invoice details or null if not found</returns>
        Task<ExportInvoiceResponse?> QueryExportInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export invoice types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export invoice types</returns>
        Task<List<ParameterItem>> GetExportInvoiceTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export document types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export document types</returns>
        Task<List<ParameterItem>> GetExportDocumentTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export currencies
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export currencies</returns>
        Task<List<ParameterItem>> GetExportCurrenciesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export destinations
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export destinations</returns>
        Task<List<ParameterItem>> GetExportDestinationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export incoterms
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export incoterms</returns>
        Task<List<ParameterItem>> GetExportIncotermsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export languages
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export languages</returns>
        Task<List<ParameterItem>> GetExportLanguagesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets export units of measurement
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of export units of measurement</returns>
        Task<List<ParameterItem>> GetExportUnitsOfMeasurementAsync(CancellationToken cancellationToken = default);
    }
} 