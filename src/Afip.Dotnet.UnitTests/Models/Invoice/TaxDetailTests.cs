using System;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Xunit;

namespace Afip.Dotnet.UnitTests.Models.Invoice
{
    public class TaxDetailTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldSetProperties()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = 1,
                Description = "Test Tax",
                BaseAmount = 100.00m,
                Rate = 0.05m,
                Amount = 5.00m
            };

            // Assert
            Assert.Equal(1, taxDetail.TaxId);
            Assert.Equal("Test Tax", taxDetail.Description);
            Assert.Equal(100.00m, taxDetail.BaseAmount);
            Assert.Equal(0.05m, taxDetail.Rate);
            Assert.Equal(5.00m, taxDetail.Amount);
        }

        [Fact]
        public void Constructor_WithDefaultValues_ShouldSetDefaults()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail();

            // Assert
            Assert.Equal(0, taxDetail.TaxId);
            Assert.Equal(string.Empty, taxDetail.Description);
            Assert.Equal(0m, taxDetail.BaseAmount);
            Assert.Equal(0m, taxDetail.Rate);
            Assert.Equal(0m, taxDetail.Amount);
        }

        [Fact]
        public void TaxDetail_WithNegativeValues_ShouldAcceptThem()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = -1,
                Description = "Negative Tax",
                BaseAmount = -100.00m,
                Rate = -0.05m,
                Amount = -5.00m
            };

            // Assert
            Assert.Equal(-1, taxDetail.TaxId);
            Assert.Equal("Negative Tax", taxDetail.Description);
            Assert.Equal(-100.00m, taxDetail.BaseAmount);
            Assert.Equal(-0.05m, taxDetail.Rate);
            Assert.Equal(-5.00m, taxDetail.Amount);
        }

        [Fact]
        public void TaxDetail_WithZeroValues_ShouldAcceptThem()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = 0,
                Description = "Zero Tax",
                BaseAmount = 0.00m,
                Rate = 0.00m,
                Amount = 0.00m
            };

            // Assert
            Assert.Equal(0, taxDetail.TaxId);
            Assert.Equal("Zero Tax", taxDetail.Description);
            Assert.Equal(0.00m, taxDetail.BaseAmount);
            Assert.Equal(0.00m, taxDetail.Rate);
            Assert.Equal(0.00m, taxDetail.Amount);
        }

        [Fact]
        public void TaxDetail_WithLargeValues_ShouldAcceptThem()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = int.MaxValue,
                Description = "Large Tax",
                BaseAmount = decimal.MaxValue,
                Rate = decimal.MaxValue,
                Amount = decimal.MaxValue
            };

            // Assert
            Assert.Equal(int.MaxValue, taxDetail.TaxId);
            Assert.Equal("Large Tax", taxDetail.Description);
            Assert.Equal(decimal.MaxValue, taxDetail.BaseAmount);
            Assert.Equal(decimal.MaxValue, taxDetail.Rate);
            Assert.Equal(decimal.MaxValue, taxDetail.Amount);
        }

        [Fact]
        public void TaxDetail_WithNullDescription_ShouldAcceptIt()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = 1,
                Description = null!,
                BaseAmount = 100.00m,
                Rate = 0.05m,
                Amount = 5.00m
            };

            // Assert
            Assert.Equal(1, taxDetail.TaxId);
            Assert.Null(taxDetail.Description);
            Assert.Equal(100.00m, taxDetail.BaseAmount);
            Assert.Equal(0.05m, taxDetail.Rate);
            Assert.Equal(5.00m, taxDetail.Amount);
        }

        [Fact]
        public void TaxDetail_WithEmptyDescription_ShouldAcceptIt()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = 1,
                Description = "",
                BaseAmount = 100.00m,
                Rate = 0.05m,
                Amount = 5.00m
            };

            // Assert
            Assert.Equal(1, taxDetail.TaxId);
            Assert.Equal("", taxDetail.Description);
            Assert.Equal(100.00m, taxDetail.BaseAmount);
            Assert.Equal(0.05m, taxDetail.Rate);
            Assert.Equal(5.00m, taxDetail.Amount);
        }

        [Fact]
        public void TaxDetail_WithWhitespaceDescription_ShouldAcceptIt()
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = 1,
                Description = "   ",
                BaseAmount = 100.00m,
                Rate = 0.05m,
                Amount = 5.00m
            };

            // Assert
            Assert.Equal(1, taxDetail.TaxId);
            Assert.Equal("   ", taxDetail.Description);
            Assert.Equal(100.00m, taxDetail.BaseAmount);
            Assert.Equal(0.05m, taxDetail.Rate);
            Assert.Equal(5.00m, taxDetail.Amount);
        }

        [Theory]
        [InlineData(1, "IVA")]
        [InlineData(2, "IIBB")]
        [InlineData(3, "Ganancias")]
        [InlineData(4, "SUSS")]
        [InlineData(5, "SIPA")]
        [InlineData(6, "Otros")]
        public void TaxDetail_CommonTaxIds_ShouldAcceptValidValues(int taxId, string description)
        {
            // Arrange & Act
            var taxDetail = new TaxDetail
            {
                TaxId = taxId,
                Description = description,
                BaseAmount = 100.00m,
                Rate = 0.05m,
                Amount = 5.00m
            };

            // Assert
            Assert.Equal(taxId, taxDetail.TaxId);
            Assert.Equal(description, taxDetail.Description);
        }
    }
} 