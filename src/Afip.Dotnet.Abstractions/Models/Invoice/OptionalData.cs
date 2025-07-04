namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Represents optional data for invoices (regulatory requirements)
    /// </summary>
    public class OptionalData
    {
        /// <summary>
        /// Optional data type ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Optional data value
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}