using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Afip.Dotnet.UnitTests.Services
{
    public class Wsfev1ServiceTests
    {
        private readonly Mock<ILogger<Wsfev1Service>> _loggerMock;
        private readonly Mock<IWsaaService> _wsaaServiceMock;
        private readonly AfipConfiguration _configuration;
        private readonly Wsfev1Service _service;

        public Wsfev1ServiceTests()
        {
            _loggerMock = new Mock<ILogger<Wsfev1Service>>();
            _wsaaServiceMock = new Mock<IWsaaService>();
            _configuration = new AfipConfiguration
            {
                Environment = AfipEnvironment.Testing,
                Cuit = 20123456789,
                CertificatePath = "test.p12",
                CertificatePassword = "password",
                TimeoutSeconds = 30
            };
            _service = new Wsfev1Service(_configuration, _wsaaServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_service);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new Wsfev1Service(null!, _wsaaServiceMock.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullWsaaService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new Wsfev1Service(_configuration, null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => 
                new Wsfev1Service(_configuration, _wsaaServiceMock.Object, null));
            Assert.Null(exception);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task AuthorizeInvoiceAsync_WithValidRequest_ShouldReturnResponse()
        {
            // Arrange
            var request = new InvoiceRequest
            {
                PointOfSale = 1,
                InvoiceType = 1,
                Concept = 1,
                DocumentType = 80,
                DocumentNumber = 12345678,
                InvoiceDate = DateTime.Today,
                CurrencyId = "PES",
                CurrencyRate = 1.0m,
                NetAmount = 1000.0m,
                VatAmount = 210.0m,
                TotalAmount = 1210.0m,
                VatDetails = new List<VatDetail>
                {
                    new VatDetail
                    {
                        VatId = 5,
                        BaseAmount = 1000.0m,
                        Amount = 210.0m
                    }
                }
            };

            var ticket = new AfipAuthTicket
            {
                Token = "test-token",
                Sign = "test-sign",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.AuthorizeInvoiceAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.PointOfSale, result.PointOfSale);
            Assert.Equal(request.InvoiceType, result.InvoiceType);
            Assert.True(result.InvoiceNumber > 0);
            Assert.Equal("A", result.Result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task AuthorizeInvoicesAsync_WithValidRequests_ShouldReturnResponses()
        {
            // Arrange
            var requests = new List<InvoiceRequest>
            {
                new InvoiceRequest
                {
                    PointOfSale = 1,
                    InvoiceType = 1,
                    Concept = 1,
                    DocumentType = 80,
                    DocumentNumber = 12345678,
                    InvoiceDate = DateTime.Today,
                    CurrencyId = "PES",
                    CurrencyRate = 1.0m,
                    NetAmount = 1000.0m,
                    VatAmount = 210.0m,
                    TotalAmount = 1210.0m,
                    VatDetails = new List<VatDetail>
                    {
                        new VatDetail
                        {
                            VatId = 5,
                            BaseAmount = 1000.0m,
                            Amount = 210.0m
                        }
                    }
                }
            };

            var ticket = new AfipAuthTicket
            {
                Token = "test-token",
                Sign = "test-sign",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.AuthorizeInvoicesAsync(requests);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("A", result[0].Result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetLastInvoiceNumberAsync_WithValidParameters_ShouldReturnNumber()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                Token = "test-token",
                Sign = "test-sign",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.GetLastInvoiceNumberAsync(1, 1);

            // Assert
            Assert.True(result > 0);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task QueryInvoiceAsync_WithValidParameters_ShouldReturnResponse()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                Token = "test-token",
                Sign = "test-sign",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.QueryInvoiceAsync(1, 1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PointOfSale);
            Assert.Equal(1, result.InvoiceType);
            Assert.Equal(1, result.InvoiceNumber);
        }

        [Fact]
        public async Task CheckServiceStatusAsync_WithValidTicket_ShouldReturnStatus()
        {
            // Arrange
            var ticket = new AfipAuthTicket
            {
                Token = "test-token",
                Sign = "test-sign",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.CheckServiceStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceStatus>(result);
        }

        [Fact]
        public async Task AuthorizeInvoiceAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
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

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("Test error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.AuthorizeInvoiceAsync(request));
        }

        [Fact]
        public async Task AuthorizeInvoicesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            var requests = new List<InvoiceRequest>
            {
                new InvoiceRequest
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
                }
            };

            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("Test error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.AuthorizeInvoicesAsync(requests));
        }

        [Fact]
        public async Task GetLastInvoiceNumberAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("Test error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetLastInvoiceNumberAsync(1, 1));
        }

        [Fact]
        public async Task QueryInvoiceAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("Test error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.QueryInvoiceAsync(1, 1, 1));
        }

        [Fact]
        public async Task CheckServiceStatusAsync_WhenWsaaServiceThrows_ShouldReturnErrorStatus()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("Test error"));

            // Act
            var result = await _service.CheckServiceStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceStatus>(result);
        }
    }
} 