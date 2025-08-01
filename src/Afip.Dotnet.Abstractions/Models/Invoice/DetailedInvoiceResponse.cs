using System;
using System.Collections.Generic;

namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Response model for detailed invoice authorization (with item details)
    /// </summary>
    public class DetailedInvoiceResponse
    {
        /// <summary>
        /// CAE (Electronic Authorization Code)
        /// </summary>
        public string? CAE { get; set; }

        /// <summary>
        /// CAE expiration date
        /// </summary>
        public DateTime? CAEExpirationDate { get; set; }

        /// <summary>
        /// Invoice number
        /// </summary>
        public long InvoiceNumber { get; set; }

        /// <summary>
        /// Point of sale number
        /// </summary>
        public int PointOfSale { get; set; }

        /// <summary>
        /// Invoice type
        /// </summary>
        public int InvoiceType { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Net amount
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// VAT amount
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Currency ID
        /// </summary>
        public string CurrencyId { get; set; } = string.Empty;

        /// <summary>
        /// Currency exchange rate
        /// </summary>
        public decimal CurrencyRate { get; set; }

        /// <summary>
        /// Invoice items with detailed information
        /// </summary>
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

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
        /// Associated invoices
        /// </summary>
        public List<AssociatedInvoice> AssociatedInvoices { get; set; } = new List<AssociatedInvoice>();

        /// <summary>
        /// Processing result
        /// </summary>
        public string ProcessingResult { get; set; } = string.Empty;

        /// <summary>
        /// Processing observations
        /// </summary>
        public List<string> ProcessingObservations { get; set; } = new List<string>();

        /// <summary>
        /// Result (A, R, O, etc)
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// Observations
        /// </summary>
        public List<Observation> Observations { get; set; } = new List<Observation>();
    }
} 