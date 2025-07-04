using System;
using System.Collections.Generic;

namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Request model for export invoice authorization
    /// </summary>
    public class ExportInvoiceRequest
    {
        /// <summary>
        /// Point of sale number
        /// </summary>
        public int PointOfSale { get; set; }

        /// <summary>
        /// Invoice type (E for exports)
        /// </summary>
        public int InvoiceType { get; set; }

        /// <summary>
        /// Invoice number from
        /// </summary>
        public long InvoiceNumberFrom { get; set; }

        /// <summary>
        /// Invoice number to
        /// </summary>
        public long InvoiceNumberTo { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Document type of the receiver
        /// </summary>
        public int DocumentType { get; set; }

        /// <summary>
        /// Document number of the receiver
        /// </summary>
        public long DocumentNumber { get; set; }

        /// <summary>
        /// Receiver name
        /// </summary>
        public string ReceiverName { get; set; } = string.Empty;

        /// <summary>
        /// Receiver address
        /// </summary>
        public string ReceiverAddress { get; set; } = string.Empty;

        /// <summary>
        /// Receiver city
        /// </summary>
        public string ReceiverCity { get; set; } = string.Empty;

        /// <summary>
        /// Receiver country code
        /// </summary>
        public string ReceiverCountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Currency ID (e.g., "DOL" for USD)
        /// </summary>
        public string CurrencyId { get; set; } = string.Empty;

        /// <summary>
        /// Currency exchange rate
        /// </summary>
        public decimal CurrencyRate { get; set; }

        /// <summary>
        /// Total amount in foreign currency
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Net amount in foreign currency
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// VAT amount in foreign currency
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Export destination code
        /// </summary>
        public int ExportDestination { get; set; }

        /// <summary>
        /// Incoterm code
        /// </summary>
        public int Incoterm { get; set; }

        /// <summary>
        /// Language code
        /// </summary>
        public int Language { get; set; }

        /// <summary>
        /// VAT details
        /// </summary>
        public List<VatDetail> VatDetails { get; set; } = new List<VatDetail>();

        /// <summary>
        /// Tax details
        /// </summary>
        public List<TaxDetail> TaxDetails { get; set; } = new List<TaxDetail>();

        /// <summary>
        /// Optional data
        /// </summary>
        public List<OptionalData> OptionalData { get; set; } = new List<OptionalData>();

        /// <summary>
        /// Associated invoices (for credit/debit notes)
        /// </summary>
        public List<AssociatedInvoice> AssociatedInvoices { get; set; } = new List<AssociatedInvoice>();
    }
} 