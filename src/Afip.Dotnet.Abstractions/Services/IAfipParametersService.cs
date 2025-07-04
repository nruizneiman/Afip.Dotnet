using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Interface for querying AFIP parameter tables (invoice types, currencies, VAT rates, etc.)
    /// </summary>
    public interface IAfipParametersService
    {
        /// <summary>
        /// Gets available invoice types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of invoice types</returns>
        Task<List<ParameterItem>> GetInvoiceTypesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets available document types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of document types</returns>
        Task<List<ParameterItem>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets available concept types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of concept types</returns>
        Task<List<ParameterItem>> GetConceptTypesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets available VAT rates
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of VAT rates</returns>
        Task<List<ParameterItem>> GetVatRatesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets available currencies
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of currencies</returns>
        Task<List<ParameterItem>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets available tax types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of tax types</returns>
        Task<List<ParameterItem>> GetTaxTypesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets currency exchange rate for a specific date
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        /// <param name="date">Date for the exchange rate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Exchange rate</returns>
        Task<decimal> GetCurrencyRateAsync(string currencyId, System.DateTime date, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets VAT conditions for receiver by invoice class
        /// </summary>
        /// <param name="invoiceClass">Invoice class (A, B, C, etc.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of VAT conditions</returns>
        Task<List<ParameterItem>> GetReceiverVatConditionsAsync(string invoiceClass, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Represents a parameter item from AFIP tables
    /// </summary>
    public class ParameterItem
    {
        /// <summary>
        /// Parameter ID/Code
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Parameter description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Valid from date
        /// </summary>
        public System.DateTime? ValidFrom { get; set; }
        
        /// <summary>
        /// Valid to date
        /// </summary>
        public System.DateTime? ValidTo { get; set; }
        
        /// <summary>
        /// Whether the parameter is currently active
        /// </summary>
        public bool IsActive => !ValidTo.HasValue || ValidTo.Value > System.DateTime.Now;
    }
}