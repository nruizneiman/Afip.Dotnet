using System;
using System.Collections.Generic;

namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Represents a request to authorize an electronic invoice
    /// </summary>
    public class InvoiceRequest
    {
        /// <summary>
        /// Point of sale number
        /// </summary>
        public int PointOfSale { get; set; }
        
        /// <summary>
        /// Invoice type code (1=Invoice A, 6=Invoice B, 11=Invoice C, etc.)
        /// </summary>
        public int InvoiceType { get; set; }
        
        /// <summary>
        /// Concept type (1=Product, 2=Service, 3=Product and Service)
        /// </summary>
        public int Concept { get; set; }
        
        /// <summary>
        /// Document type of the customer (80=CUIT, 96=DNI, etc.)
        /// </summary>
        public int DocumentType { get; set; }
        
        /// <summary>
        /// Document number of the customer
        /// </summary>
        public long DocumentNumber { get; set; }
        
        /// <summary>
        /// Invoice number from
        /// </summary>
        public long InvoiceNumberFrom { get; set; }
        
        /// <summary>
        /// Invoice number to
        /// </summary>
        public long InvoiceNumberTo { get; set; }
        
        /// <summary>
        /// Invoice date (YYYYMMDD format)
        /// </summary>
        public DateTime InvoiceDate { get; set; }
        
        /// <summary>
        /// Total amount
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Non-taxable amount
        /// </summary>
        public decimal NonTaxableAmount { get; set; }
        
        /// <summary>
        /// Net taxable amount
        /// </summary>
        public decimal NetAmount { get; set; }
        
        /// <summary>
        /// Exempt amount
        /// </summary>
        public decimal ExemptAmount { get; set; }
        
        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// VAT amount
        /// </summary>
        public decimal VatAmount { get; set; }
        
        /// <summary>
        /// Currency ID (PES=Argentine Peso, DOL=US Dollar, etc.)
        /// </summary>
        public string CurrencyId { get; set; } = "PES";
        
        /// <summary>
        /// Currency exchange rate
        /// </summary>
        public decimal CurrencyRate { get; set; } = 1.0m;
        
        /// <summary>
        /// Service date from (required for services)
        /// </summary>
        public DateTime? ServiceDateFrom { get; set; }
        
        /// <summary>
        /// Service date to (required for services)
        /// </summary>
        public DateTime? ServiceDateTo { get; set; }
        
        /// <summary>
        /// Payment due date (required for services)
        /// </summary>
        public DateTime? PaymentDueDate { get; set; }
        
        /// <summary>
        /// VAT details
        /// </summary>
        public List<VatDetail> VatDetails { get; set; } = new List<VatDetail>();
        
        /// <summary>
        /// Tax details
        /// </summary>
        public List<TaxDetail> TaxDetails { get; set; } = new List<TaxDetail>();
        
        /// <summary>
        /// Associated invoices (for credit/debit notes)
        /// </summary>
        public List<AssociatedInvoice> AssociatedInvoices { get; set; } = new List<AssociatedInvoice>();
        
        /// <summary>
        /// Optional data
        /// </summary>
        public List<OptionalData> OptionalData { get; set; } = new List<OptionalData>();
        
        /// <summary>
        /// Whether the invoice is paid in the same foreign currency
        /// </summary>
        public bool PayInSameForeignCurrency { get; set; } = false;
        
        /// <summary>
        /// VAT condition of the receiver (required from FEv4)
        /// </summary>
        public int? ReceiverVatCondition { get; set; }
    }
}