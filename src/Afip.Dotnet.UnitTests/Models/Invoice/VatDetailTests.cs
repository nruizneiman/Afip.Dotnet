using System;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Xunit;

namespace Afip.Dotnet.UnitTests.Models.Invoice
{
    public class VatDetailTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldSetProperties()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = 5,
                BaseAmount = 82.64m,
                Amount = 17.36m
            };

            // Assert
            Assert.Equal(5, vatDetail.VatId);
            Assert.Equal(82.64m, vatDetail.BaseAmount);
            Assert.Equal(17.36m, vatDetail.Amount);
        }

        [Fact]
        public void Constructor_WithDefaultValues_ShouldSetDefaults()
        {
            // Arrange & Act
            var vatDetail = new VatDetail();

            // Assert
            Assert.Equal(0, vatDetail.VatId);
            Assert.Equal(0m, vatDetail.BaseAmount);
            Assert.Equal(0m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithNegativeValues_ShouldAcceptThem()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = -1,
                BaseAmount = -100.00m,
                Amount = -21.00m
            };

            // Assert
            Assert.Equal(-1, vatDetail.VatId);
            Assert.Equal(-100.00m, vatDetail.BaseAmount);
            Assert.Equal(-21.00m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithZeroValues_ShouldAcceptThem()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = 0,
                BaseAmount = 0.00m,
                Amount = 0.00m
            };

            // Assert
            Assert.Equal(0, vatDetail.VatId);
            Assert.Equal(0.00m, vatDetail.BaseAmount);
            Assert.Equal(0.00m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithLargeValues_ShouldAcceptThem()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = int.MaxValue,
                BaseAmount = decimal.MaxValue,
                Amount = decimal.MaxValue
            };

            // Assert
            Assert.Equal(int.MaxValue, vatDetail.VatId);
            Assert.Equal(decimal.MaxValue, vatDetail.BaseAmount);
            Assert.Equal(decimal.MaxValue, vatDetail.Amount);
        }

        [Theory]
        [InlineData(3, "IVA 0%")]
        [InlineData(4, "IVA 10.5%")]
        [InlineData(5, "IVA 21%")]
        [InlineData(6, "IVA 27%")]
        [InlineData(8, "IVA 5%")]
        [InlineData(9, "IVA 2.5%")]
        public void VatDetail_CommonVatIds_ShouldAcceptValidValues(int vatId, string description)
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = vatId,
                BaseAmount = 100.00m,
                Amount = 21.00m
            };

            // Assert
            Assert.Equal(vatId, vatDetail.VatId);
            Assert.Equal(100.00m, vatDetail.BaseAmount);
            Assert.Equal(21.00m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithExemptVat_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = 3, // IVA 0%
                BaseAmount = 100.00m,
                Amount = 0.00m
            };

            // Assert
            Assert.Equal(3, vatDetail.VatId);
            Assert.Equal(100.00m, vatDetail.BaseAmount);
            Assert.Equal(0.00m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithStandardVat_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = 5, // IVA 21%
                BaseAmount = 100.00m,
                Amount = 21.00m
            };

            // Assert
            Assert.Equal(5, vatDetail.VatId);
            Assert.Equal(100.00m, vatDetail.BaseAmount);
            Assert.Equal(21.00m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithReducedVat_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = 4, // IVA 10.5%
                BaseAmount = 100.00m,
                Amount = 10.50m
            };

            // Assert
            Assert.Equal(4, vatDetail.VatId);
            Assert.Equal(100.00m, vatDetail.BaseAmount);
            Assert.Equal(10.50m, vatDetail.Amount);
        }

        [Fact]
        public void VatDetail_WithIncreasedVat_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var vatDetail = new VatDetail
            {
                VatId = 6, // IVA 27%
                BaseAmount = 100.00m,
                Amount = 27.00m
            };

            // Assert
            Assert.Equal(6, vatDetail.VatId);
            Assert.Equal(100.00m, vatDetail.BaseAmount);
            Assert.Equal(27.00m, vatDetail.Amount);
        }
    }
} 