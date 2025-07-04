using System;
using System.Collections.Generic;

namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Request model for detailed invoice authorization (with item details)
    /// </summary>
    public class DetailedInvoiceRequest
    {
        /// <summary>
        /// Point of sale number
        /// </summary>
        public int PointOfSale { get; set; }

        /// <summary>
        /// Invoice type
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
        /// Receiver postal code
        /// </summary>
        public string ReceiverPostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Receiver VAT condition
        /// </summary>
        public int ReceiverVatCondition { get; set; }

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
        /// Currency ID (optional, defaults to ARS)
        /// </summary>
        public string CurrencyId { get; set; } = "PES";

        /// <summary>
        /// Currency exchange rate (optional, defaults to 1)
        /// </summary>
        public decimal CurrencyRate { get; set; } = 1.0m;

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
        /// Associated invoices (for credit/debit notes)
        /// </summary>
        public List<AssociatedInvoice> AssociatedInvoices { get; set; } = new List<AssociatedInvoice>();
    }

    /// <summary>
    /// Represents an invoice item with detailed information
    /// </summary>
    public class InvoiceItem
    {
        /// <summary>
        /// Item description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Item quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total amount for this item
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Net amount for this item
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// VAT amount for this item
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Unit of measurement code
        /// </summary>
        public int UnitOfMeasurement { get; set; }

        /// <summary>
        /// Item category code
        /// </summary>
        public int ItemCategory { get; set; }

        /// <summary>
        /// Item type code
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// VAT rate for this item
        /// </summary>
        public int VatRate { get; set; }

        /// <summary>
        /// Item taxes
        /// </summary>
        public List<ItemTax> Taxes { get; set; } = new List<ItemTax>();

        /// <summary>
        /// Item discounts
        /// </summary>
        public List<ItemDiscount> Discounts { get; set; } = new List<ItemDiscount>();

        /// <summary>
        /// Item optional data
        /// </summary>
        public List<OptionalData> OptionalData { get; set; } = new List<OptionalData>();
    }

    /// <summary>
    /// Represents a tax applied to an item
    /// </summary>
    public class ItemTax
    {
        /// <summary>
        /// Tax type code
        /// </summary>
        public int TaxId { get; set; }

        /// <summary>
        /// Tax description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tax rate
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Tax base amount
        /// </summary>
        public decimal BaseAmount { get; set; }
    }

    /// <summary>
    /// Represents a discount applied to an item
    /// </summary>
    public class ItemDiscount
    {
        /// <summary>
        /// Discount type code
        /// </summary>
        public int DiscountType { get; set; }

        /// <summary>
        /// Discount description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Discount rate
        /// </summary>
        public decimal DiscountRate { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Discount base amount
        /// </summary>
        public decimal BaseAmount { get; set; }
    }
} 