using System;

namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Represents an associated invoice for credit/debit notes
    /// </summary>
    public class AssociatedInvoice
    {
        /// <summary>
        /// Invoice type code
        /// </summary>
        public int InvoiceType { get; set; }
        
        /// <summary>
        /// Point of sale number
        /// </summary>
        public int PointOfSale { get; set; }
        
        /// <summary>
        /// Invoice number
        /// </summary>
        public long InvoiceNumber { get; set; }
        
        /// <summary>
        /// CUIT of the issuer (for FCE invoices)
        /// </summary>
        public long? Cuit { get; set; }
        
        /// <summary>
        /// Date of the associated invoice
        /// </summary>
        public DateTime? Date { get; set; }
    }
}