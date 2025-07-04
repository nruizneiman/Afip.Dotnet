namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Represents VAT details for an invoice
    /// </summary>
    public class VatDetail
    {
        /// <summary>
        /// VAT rate ID (3=0%, 4=10.5%, 5=21%, 6=27%, 8=5%, 9=2.5%)
        /// </summary>
        public int VatId { get; set; }
        
        /// <summary>
        /// Base amount on which VAT is calculated
        /// </summary>
        public decimal BaseAmount { get; set; }
        
        /// <summary>
        /// VAT amount
        /// </summary>
        public decimal Amount { get; set; }
    }
}