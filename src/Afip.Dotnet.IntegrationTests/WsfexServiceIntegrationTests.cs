using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Afip.Dotnet.Services;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Sdk;

namespace Afip.Dotnet.IntegrationTests
{
    public class WsfexServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly WsfexService _service;

        public WsfexServiceIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            
            // Only create service if LoggerFactory is available
            if (_fixture.LoggerFactory != null)
            {
                _service = new WsfexService(
                    _fixture.WsaaService,
                    _fixture.ParametersService,
                    _fixture.LoggerFactory.CreateLogger<WsfexService>(),
                    _fixture.Configuration);
            }
            else
            {
                _service = null;
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task CheckServiceStatusAsync_ShouldReturnServiceStatus()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.CheckServiceStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AppServer);
            Assert.NotNull(result.DbServer);
            Assert.NotNull(result.AuthServer);
            Assert.NotNull(result.StatusMessage);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportInvoiceTypesAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportInvoiceTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportDocumentTypesAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportDocumentTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportCurrenciesAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportCurrenciesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportDestinationsAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportDestinationsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportIncotermsAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportIncotermsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportLanguagesAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportLanguagesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetExportUnitsOfMeasurementAsync_ShouldReturnParameterList()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetExportUnitsOfMeasurementAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            foreach (var item in result)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Description);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task GetLastInvoiceNumberAsync_ShouldReturnValidNumber()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.GetLastInvoiceNumberAsync(1, 1);

            // Assert
            Assert.True(result >= 0);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task AuthorizeExportInvoiceAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Arrange
            var request = new ExportInvoiceRequest
            {
                PointOfSale = 1,
                InvoiceType = 1,
                InvoiceNumberFrom = 1,
                InvoiceNumberTo = 1,
                InvoiceDate = DateTime.Today,
                DocumentType = 80,
                DocumentNumber = 12345678,
                ReceiverName = "Test Receiver",
                ReceiverAddress = "Test Address",
                ReceiverCity = "Test City",
                ReceiverCountryCode = "AR",
                CurrencyId = "DOL",
                CurrencyRate = 1.0m,
                TotalAmount = 100.0m,
                NetAmount = 82.64m,
                VatAmount = 17.36m,
                ExportDestination = 1,
                Incoterm = 1,
                Language = 1,
                VatDetails = new List<VatDetail>
                {
                    new VatDetail
                    {
                        VatId = 5,
                        BaseAmount = 82.64m,
                        Amount = 17.36m
                    }
                }
            };

            // Act
            var result = await _service.AuthorizeExportInvoiceAsync(request);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Service", "WSFEX")]
        public async Task QueryExportInvoiceAsync_WithNonExistentInvoice_ShouldReturnNull()
        {
            // Skip if service is not available
            if (_service == null)
            {
                return;
            }
            
            // Skip if no certificate is available
            if (!_fixture.HasCertificate)
            {
                return;
            }

            // Act
            var result = await _service.QueryExportInvoiceAsync(1, 1, 999999);

            // Assert
            Assert.Null(result);
        }
    }
} 