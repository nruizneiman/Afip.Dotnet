using System;
using System.Collections.Generic;
using Xunit;
using Afip.Dotnet.Abstractions.Models.Invoice;

namespace Afip.Dotnet.UnitTests.Models.Invoice
{
    public class InvoiceRequestTests
    {
        [Fact]
        public void InvoiceRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new InvoiceRequest();

            // Assert
            Assert.Equal(0, request.PointOfSale);
            Assert.Equal(0, request.InvoiceType);
            Assert.Equal(0, request.Concept);
            Assert.Equal(0, request.DocumentType);
            Assert.Equal(0, request.DocumentNumber);
            Assert.Equal(0, request.InvoiceNumberFrom);
            Assert.Equal(0, request.InvoiceNumberTo);
            Assert.Equal(default(DateTime), request.InvoiceDate);
            Assert.Equal(0m, request.TotalAmount);
            Assert.Equal(0m, request.NonTaxableAmount);
            Assert.Equal(0m, request.NetAmount);
            Assert.Equal(0m, request.ExemptAmount);
            Assert.Equal(0m, request.TaxAmount);
            Assert.Equal(0m, request.VatAmount);
            Assert.Equal("PES", request.CurrencyId);
            Assert.Equal(1.0m, request.CurrencyRate);
            Assert.Null(request.ServiceDateFrom);
            Assert.Null(request.ServiceDateTo);
            Assert.Null(request.PaymentDueDate);
            Assert.NotNull(request.VatDetails);
            Assert.Empty(request.VatDetails);
            Assert.NotNull(request.TaxDetails);
            Assert.Empty(request.TaxDetails);
            Assert.NotNull(request.AssociatedInvoices);
            Assert.Empty(request.AssociatedInvoices);
            Assert.NotNull(request.OptionalData);
            Assert.Empty(request.OptionalData);
            Assert.False(request.PayInSameForeignCurrency);
            Assert.Null(request.ReceiverVatCondition);
        }

        [Fact]
        public void InvoiceRequest_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var invoiceDate = DateTime.Today;
            var serviceFrom = DateTime.Today.AddDays(-7);
            var serviceTo = DateTime.Today;
            var paymentDue = DateTime.Today.AddDays(30);

            var vatDetails = new List<VatDetail>
            {
                new VatDetail { VatId = 5, BaseAmount = 100m, Amount = 21m }
            };

            var taxDetails = new List<TaxDetail>
            {
                new TaxDetail { TaxId = 1, Description = "Test Tax", BaseAmount = 100m, Rate = 5m, Amount = 5m }
            };

            var associatedInvoices = new List<AssociatedInvoice>
            {
                new AssociatedInvoice { InvoiceType = 1, PointOfSale = 1, InvoiceNumber = 123L }
            };

            var optionalData = new List<OptionalData>
            {
                new OptionalData { Id = 1, Value = "Test Value" }
            };

            // Act
            var request = new InvoiceRequest
            {
                PointOfSale = 1,
                InvoiceType = 11,
                Concept = 1,
                DocumentType = 96,
                DocumentNumber = 12345678,
                InvoiceNumberFrom = 1,
                InvoiceNumberTo = 1,
                InvoiceDate = invoiceDate,
                TotalAmount = 121m,
                NonTaxableAmount = 0m,
                NetAmount = 100m,
                ExemptAmount = 0m,
                TaxAmount = 5m,
                VatAmount = 21m,
                CurrencyId = "DOL",
                CurrencyRate = 350.50m,
                ServiceDateFrom = serviceFrom,
                ServiceDateTo = serviceTo,
                PaymentDueDate = paymentDue,
                VatDetails = vatDetails,
                TaxDetails = taxDetails,
                AssociatedInvoices = associatedInvoices,
                OptionalData = optionalData,
                PayInSameForeignCurrency = true,
                ReceiverVatCondition = 1
            };

            // Assert
            Assert.Equal(1, request.PointOfSale);
            Assert.Equal(11, request.InvoiceType);
            Assert.Equal(1, request.Concept);
            Assert.Equal(96, request.DocumentType);
            Assert.Equal(12345678, request.DocumentNumber);
            Assert.Equal(1, request.InvoiceNumberFrom);
            Assert.Equal(1, request.InvoiceNumberTo);
            Assert.Equal(invoiceDate, request.InvoiceDate);
            Assert.Equal(121m, request.TotalAmount);
            Assert.Equal(0m, request.NonTaxableAmount);
            Assert.Equal(100m, request.NetAmount);
            Assert.Equal(0m, request.ExemptAmount);
            Assert.Equal(5m, request.TaxAmount);
            Assert.Equal(21m, request.VatAmount);
            Assert.Equal("DOL", request.CurrencyId);
            Assert.Equal(350.50m, request.CurrencyRate);
            Assert.Equal(serviceFrom, request.ServiceDateFrom);
            Assert.Equal(serviceTo, request.ServiceDateTo);
            Assert.Equal(paymentDue, request.PaymentDueDate);
            Assert.Equal(vatDetails, request.VatDetails);
            Assert.Equal(taxDetails, request.TaxDetails);
            Assert.Equal(associatedInvoices, request.AssociatedInvoices);
            Assert.Equal(optionalData, request.OptionalData);
            Assert.True(request.PayInSameForeignCurrency);
            Assert.Equal(1, request.ReceiverVatCondition);
        }

        [Theory]
        [InlineData(1, "Invoice A")]
        [InlineData(6, "Invoice B")]
        [InlineData(11, "Invoice C")]
        [InlineData(201, "Credit Note A")]
        [InlineData(206, "Credit Note B")]
        [InlineData(211, "Credit Note C")]
        public void InvoiceRequest_CommonInvoiceTypes_ShouldAcceptValidValues(int invoiceType, string description)
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                InvoiceType = invoiceType
            };

            // Assert
            Assert.Equal(invoiceType, request.InvoiceType);
        }

        [Theory]
        [InlineData(1, "Products")]
        [InlineData(2, "Services")]
        [InlineData(3, "Products and Services")]
        public void InvoiceRequest_ConceptTypes_ShouldAcceptValidValues(int concept, string description)
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                Concept = concept
            };

            // Assert
            Assert.Equal(concept, request.Concept);
        }

        [Theory]
        [InlineData(80, "CUIT")]
        [InlineData(96, "DNI")]
        [InlineData(99, "General Document")]
        public void InvoiceRequest_DocumentTypes_ShouldAcceptValidValues(int documentType, string description)
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                DocumentType = documentType
            };

            // Assert
            Assert.Equal(documentType, request.DocumentType);
        }

        [Fact]
        public void InvoiceRequest_VatDetails_CanAddMultipleEntries()
        {
            // Arrange
            var request = new InvoiceRequest();
            var vatDetail1 = new VatDetail { VatId = 5, BaseAmount = 100m, Amount = 21m };
            var vatDetail2 = new VatDetail { VatId = 3, BaseAmount = 50m, Amount = 0m };

            // Act
            request.VatDetails.Add(vatDetail1);
            request.VatDetails.Add(vatDetail2);

            // Assert
            Assert.Equal(2, request.VatDetails.Count);
            Assert.Contains(vatDetail1, request.VatDetails);
            Assert.Contains(vatDetail2, request.VatDetails);
        }

        [Fact]
        public void InvoiceRequest_ForeignCurrencyFields_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                CurrencyId = "USD",
                CurrencyRate = 300.75m,
                PayInSameForeignCurrency = true
            };

            // Assert
            Assert.Equal("USD", request.CurrencyId);
            Assert.Equal(300.75m, request.CurrencyRate);
            Assert.True(request.PayInSameForeignCurrency);
        }

        [Fact]
        public void InvoiceRequest_ServiceDates_ShouldBeOptional()
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                Concept = 2 // Services
            };

            // Assert
            Assert.Null(request.ServiceDateFrom);
            Assert.Null(request.ServiceDateTo);
            Assert.Null(request.PaymentDueDate);
        }

        [Fact]
        public void InvoiceRequest_Collections_ShouldBeInitializedButEmpty()
        {
            // Arrange & Act
            var request = new InvoiceRequest();

            // Assert
            Assert.NotNull(request.VatDetails);
            Assert.Empty(request.VatDetails);
            Assert.NotNull(request.TaxDetails);
            Assert.Empty(request.TaxDetails);
            Assert.NotNull(request.AssociatedInvoices);
            Assert.Empty(request.AssociatedInvoices);
            Assert.NotNull(request.OptionalData);
            Assert.Empty(request.OptionalData);
        }
    }
}