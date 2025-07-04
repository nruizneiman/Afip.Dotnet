namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Represents tax details for an invoice
    /// </summary>
    public class TaxDetail
    {
        /// <summary>
        /// Tax type ID (1=National, 2=Provincial, 3=Municipal, 4=Internal, 99=Other)
        /// </summary>
        public int TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Base amount on which tax is calculated
        /// </summary>
        public decimal BaseAmount { get; set; }
        
        /// <summary>
        /// Tax rate (percentage)
        /// </summary>
        public decimal TaxRate { get; set; }
        
        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
    }
}