using System;
using System.Collections.Generic;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Xunit;

namespace Afip.Dotnet.UnitTests.Models.Invoice
{
    public class InvoiceRequestTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldSetProperties()
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                InvoiceType = 1,
                PointOfSale = 1,
                InvoiceNumberFrom = 1,
                InvoiceNumberTo = 1,
                InvoiceDate = DateTime.Today,
                Concept = 1,
                DocumentType = 80,
                DocumentNumber = 12345678,
                TotalAmount = 100.00m,
                NetAmount = 82.64m,
                ExemptAmount = 0.00m,
                VatAmount = 17.36m,
                CurrencyId = "PES",
                CurrencyRate = 1.00m
            };

            // Assert
            Assert.Equal(1, request.InvoiceType);
            Assert.Equal(1, request.PointOfSale);
            Assert.Equal(1, request.InvoiceNumberFrom);
            Assert.Equal(1, request.InvoiceNumberTo);
            Assert.Equal(DateTime.Today, request.InvoiceDate);
            Assert.Equal(1, request.Concept);
            Assert.Equal(80, request.DocumentType);
            Assert.Equal(12345678, request.DocumentNumber);
            Assert.Equal(100.00m, request.TotalAmount);
            Assert.Equal(82.64m, request.NetAmount);
            Assert.Equal(0.00m, request.ExemptAmount);
            Assert.Equal(17.36m, request.VatAmount);
            Assert.Equal("PES", request.CurrencyId);
            Assert.Equal(1.00m, request.CurrencyRate);
        }

        [Fact]
        public void Constructor_WithDefaultValues_ShouldSetDefaults()
        {
            // Arrange & Act
            var request = new InvoiceRequest();

            // Assert
            Assert.Equal(0, request.InvoiceType);
            Assert.Equal(0, request.PointOfSale);
            Assert.Equal(0, request.InvoiceNumberFrom);
            Assert.Equal(0, request.InvoiceNumberTo);
            Assert.Equal(default(DateTime), request.InvoiceDate);
            Assert.Equal(0, request.Concept);
            Assert.Equal(0, request.DocumentType);
            Assert.Equal(0, request.DocumentNumber);
            Assert.Equal(0m, request.TotalAmount);
            Assert.Equal(0m, request.NetAmount);
            Assert.Equal(0m, request.ExemptAmount);
            Assert.Equal(0m, request.VatAmount);
            Assert.Equal("PES", request.CurrencyId);
            Assert.Equal(1.0m, request.CurrencyRate);
            Assert.NotNull(request.VatDetails);
            Assert.Empty(request.VatDetails);
            Assert.NotNull(request.TaxDetails);
            Assert.Empty(request.TaxDetails);
            Assert.NotNull(request.AssociatedInvoices);
            Assert.Empty(request.AssociatedInvoices);
            Assert.NotNull(request.OptionalData);
            Assert.Empty(request.OptionalData);
        }

        [Fact]
        public void VatDetails_WithValidList_ShouldSetCorrectly()
        {
            // Arrange
            var request = new InvoiceRequest();
            var vatDetails = new List<VatDetail>
            {
                new VatDetail
                {
                    VatId = 5,
                    BaseAmount = 82.64m,
                    Amount = 17.36m
                }
            };

            // Act
            request.VatDetails = vatDetails;

            // Assert
            Assert.NotNull(request.VatDetails);
            Assert.Single(request.VatDetails);
            Assert.Equal(5, request.VatDetails[0].VatId);
            Assert.Equal(82.64m, request.VatDetails[0].BaseAmount);
            Assert.Equal(17.36m, request.VatDetails[0].Amount);
        }

        [Fact]
        public void TaxDetails_WithValidList_ShouldSetCorrectly()
        {
            // Arrange
            var request = new InvoiceRequest();
            var taxDetails = new List<TaxDetail>
            {
                new TaxDetail
                {
                    TaxId = 1,
                    Description = "Test Tax",
                    BaseAmount = 100.00m,
                    Rate = 0.05m,
                    Amount = 5.00m
                }
            };

            // Act
            request.TaxDetails = taxDetails;

            // Assert
            Assert.NotNull(request.TaxDetails);
            Assert.Single(request.TaxDetails);
            Assert.Equal(1, request.TaxDetails[0].TaxId);
            Assert.Equal("Test Tax", request.TaxDetails[0].Description);
            Assert.Equal(100.00m, request.TaxDetails[0].BaseAmount);
            Assert.Equal(0.05m, request.TaxDetails[0].Rate);
            Assert.Equal(5.00m, request.TaxDetails[0].Amount);
        }

        [Fact]
        public void AssociatedInvoices_WithValidList_ShouldSetCorrectly()
        {
            // Arrange
            var request = new InvoiceRequest();
            var associatedInvoices = new List<AssociatedInvoice>
            {
                new AssociatedInvoice
                {
                    InvoiceType = 2,
                    PointOfSale = 1,
                    InvoiceNumber = 100
                }
            };

            // Act
            request.AssociatedInvoices = associatedInvoices;

            // Assert
            Assert.NotNull(request.AssociatedInvoices);
            Assert.Single(request.AssociatedInvoices);
            Assert.Equal(2, request.AssociatedInvoices[0].InvoiceType);
            Assert.Equal(1, request.AssociatedInvoices[0].PointOfSale);
            Assert.Equal(100, request.AssociatedInvoices[0].InvoiceNumber);
        }

        [Fact]
        public void OptionalData_WithValidList_ShouldSetCorrectly()
        {
            // Arrange
            var request = new InvoiceRequest();
            var optionalData = new List<OptionalData>
            {
                new OptionalData
                {
                    Id = 1,
                    Value = "test-value"
                }
            };

            // Act
            request.OptionalData = optionalData;

            // Assert
            Assert.NotNull(request.OptionalData);
            Assert.Single(request.OptionalData);
            Assert.Equal(1, request.OptionalData[0].Id);
            Assert.Equal("test-value", request.OptionalData[0].Value);
        }

        [Theory]
        [InlineData(1, "Factura A")]
        [InlineData(2, "Nota de Débito A")]
        [InlineData(3, "Nota de Crédito A")]
        [InlineData(6, "Factura B")]
        [InlineData(7, "Nota de Débito B")]
        [InlineData(8, "Nota de Crédito B")]
        [InlineData(11, "Factura C")]
        [InlineData(12, "Nota de Débito C")]
        [InlineData(13, "Nota de Crédito C")]
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
        [InlineData(86, "CUIL")]
        [InlineData(87, "CDI")]
        [InlineData(89, "LE")]
        [InlineData(90, "LC")]
        [InlineData(91, "CI Extranjera")]
        [InlineData(92, "En Trámite")]
        [InlineData(93, "Acta Nacimiento")]
        [InlineData(94, "CI Bs. As. RNP")]
        [InlineData(95, "CI Bs. As. RENATE")]
        [InlineData(96, "DNI")]
        [InlineData(99, "Consumidor Final")]
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
        public void InvoiceRequest_WithNullCollections_ShouldNotThrow()
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                VatDetails = null!,
                TaxDetails = null!,
                AssociatedInvoices = null!,
                OptionalData = null!
            };

            // Assert
            Assert.Null(request.VatDetails);
            Assert.Null(request.TaxDetails);
            Assert.Null(request.AssociatedInvoices);
            Assert.Null(request.OptionalData);
        }

        [Fact]
        public void InvoiceRequest_WithEmptyCollections_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var request = new InvoiceRequest
            {
                VatDetails = new List<VatDetail>(),
                TaxDetails = new List<TaxDetail>(),
                AssociatedInvoices = new List<AssociatedInvoice>(),
                OptionalData = new List<OptionalData>()
            };

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