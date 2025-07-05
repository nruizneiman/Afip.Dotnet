using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Afip.Dotnet.UnitTests.Services
{
    public class AfipParametersServiceTests
    {
        private readonly Mock<ILogger<AfipParametersService>> _loggerMock;
        private readonly Mock<IWsaaService> _wsaaServiceMock;
        private readonly AfipConfiguration _configuration;
        private readonly AfipParametersService _service;

        public AfipParametersServiceTests()
        {
            _loggerMock = new Mock<ILogger<AfipParametersService>>();
            _wsaaServiceMock = new Mock<IWsaaService>();
            _configuration = new AfipConfiguration
            {
                Environment = AfipEnvironment.Testing,
                Cuit = 20123456789,
                CertificatePath = "test.p12",
                CertificatePassword = "password",
                TimeoutSeconds = 30
            };
            _service = new AfipParametersService(_configuration, _wsaaServiceMock.Object, _loggerMock.Object);
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
                new AfipParametersService(null!, _wsaaServiceMock.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullWsaaService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new AfipParametersService(_configuration, null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => 
                new AfipParametersService(_configuration, _wsaaServiceMock.Object, null));
            Assert.Null(exception);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetInvoiceTypesAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetInvoiceTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetDocumentTypesAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetDocumentTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetConceptTypesAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetConceptTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetVatRatesAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetVatRatesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetCurrenciesAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetCurrenciesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetTaxTypesAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetTaxTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetCurrencyRateAsync_WithValidTicket_ShouldReturnRate()
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
            var result = await _service.GetCurrencyRateAsync("USD", DateTime.Today);

            // Assert
            Assert.True(result > 0);
        }

        [Fact(Skip = "Requires real AFIP endpoints")]
        public async Task GetReceiverVatConditionsAsync_WithValidTicket_ShouldReturnParameterItems()
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
            var result = await _service.GetReceiverVatConditionsAsync("A");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ParameterItem>>(result);
        }

        [Fact]
        public async Task GetInvoiceTypesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetInvoiceTypesAsync());
        }

        [Fact]
        public async Task GetDocumentTypesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetDocumentTypesAsync());
        }

        [Fact]
        public async Task GetConceptTypesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetConceptTypesAsync());
        }

        [Fact]
        public async Task GetVatRatesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetVatRatesAsync());
        }

        [Fact]
        public async Task GetCurrenciesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetCurrenciesAsync());
        }

        [Fact]
        public async Task GetTaxTypesAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetTaxTypesAsync());
        }

        [Fact]
        public async Task GetCurrencyRateAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("WSAA error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetCurrencyRateAsync("DOL", DateTime.Today));
        }

        [Fact]
        public async Task GetReceiverVatConditionsAsync_WhenWsaaServiceThrows_ShouldThrowAfipException()
        {
            // Arrange
            _wsaaServiceMock.Setup(x => x.GetValidTicketAsync("wsfe", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AfipException("Test error"));

            // Act & Assert
            await Assert.ThrowsAsync<AfipException>(() => _service.GetReceiverVatConditionsAsync("A"));
        }
    }
} 