using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Service for AFIP WSMTXCA (Electronic Invoicing with Item Details) operations
    /// </summary>
    public interface IWsmtxcaService
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
        /// <param name="invoiceType">Invoice type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Last invoice number</returns>
        Task<long> GetLastInvoiceNumberAsync(int pointOfSale, int invoiceType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authorizes an invoice with item details
        /// </summary>
        /// <param name="request">Invoice request with item details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Invoice response with item details</returns>
        Task<DetailedInvoiceResponse> AuthorizeDetailedInvoiceAsync(DetailedInvoiceRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries an invoice with item details by its details
        /// </summary>
        /// <param name="pointOfSale">Point of sale number</param>
        /// <param name="invoiceType">Invoice type</param>
        /// <param name="invoiceNumber">Invoice number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Invoice details with items or null if not found</returns>
        Task<DetailedInvoiceResponse?> QueryDetailedInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets invoice types for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of invoice types</returns>
        Task<List<ParameterItem>> GetDetailedInvoiceTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets document types for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of document types</returns>
        Task<List<ParameterItem>> GetDetailedDocumentTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets VAT rates for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of VAT rates</returns>
        Task<List<ParameterItem>> GetDetailedVatRatesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets units of measurement for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of units of measurement</returns>
        Task<List<ParameterItem>> GetDetailedUnitsOfMeasurementAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets item categories for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of item categories</returns>
        Task<List<ParameterItem>> GetItemCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets item types for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of item types</returns>
        Task<List<ParameterItem>> GetItemTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets tax types for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of tax types</returns>
        Task<List<ParameterItem>> GetTaxTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets discount types for detailed invoicing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discount types</returns>
        Task<List<ParameterItem>> GetDiscountTypesAsync(CancellationToken cancellationToken = default);
    }
} 