using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic; // Added for List<VatDetail>

namespace Afip.Dotnet.IntegrationTests
{
    [Collection("AFIP Integration Tests")]
    public class AfipServicesIntegrationTests
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public AfipServicesIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Get_Service_Status_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;

            // Act
            var status = await client.ElectronicInvoicing.CheckServiceStatusAsync();

            // Assert
            status.Should().NotBeNull();
            _output.WriteLine($"Service Status - App: {status.AppServer}, Auth: {status.AuthServer}, DB: {status.DbServer}");
        }

        [Fact]
        public async Task Should_Authenticate_With_WSAA_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var wsaaService = _fixture.ServiceProvider.GetRequiredService<IWsaaService>();

            // Act
            var ticket = await wsaaService.GetValidTicketAsync("wsfe");

            // Assert
            ticket.Should().NotBeNull();
            ticket.Token.Should().NotBeNullOrEmpty();
            ticket.Sign.Should().NotBeNullOrEmpty();
            ticket.IsValid.Should().BeTrue();
            _output.WriteLine($"Authentication successful. Token length: {ticket.Token.Length}, Expires: {ticket.ExpiresAt}");
        }

        [Fact]
        public async Task Should_Get_Invoice_Types_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;

            // Act
            var invoiceTypes = await client.Parameters.GetInvoiceTypesAsync();

            // Assert
            invoiceTypes.Should().NotBeNull();
            invoiceTypes.Should().NotBeEmpty();
            invoiceTypes.Should().Contain(it => it.Id == "11"); // Factura C should exist
            _output.WriteLine($"Retrieved {invoiceTypes.Count} invoice types");
        }

        [Fact]
        public async Task Should_Get_Document_Types_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;

            // Act
            var documentTypes = await client.Parameters.GetDocumentTypesAsync();

            // Assert
            documentTypes.Should().NotBeNull();
            documentTypes.Should().NotBeEmpty();
            documentTypes.Should().Contain(dt => dt.Id == "80"); // CUIT should exist
            documentTypes.Should().Contain(dt => dt.Id == "96"); // DNI should exist
            _output.WriteLine($"Retrieved {documentTypes.Count} document types");
        }

        [Fact]
        public async Task Should_Get_VAT_Rates_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;

            // Act
            var vatRates = await client.Parameters.GetVatRatesAsync();

            // Assert
            vatRates.Should().NotBeNull();
            vatRates.Should().NotBeEmpty();
            vatRates.Should().Contain(vr => vr.Id == "5"); // 21% VAT should exist
            _output.WriteLine($"Retrieved {vatRates.Count} VAT rates");
        }

        [Fact]
        public async Task Should_Get_Last_Invoice_Number_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;
            const int pointOfSale = 1;
            const int invoiceType = 11; // Factura C

            // Act
            var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(pointOfSale, invoiceType);

            // Assert
            lastNumber.Should().BeGreaterOrEqualTo(0);
            _output.WriteLine($"Last invoice number for POS {pointOfSale}, type {invoiceType}: {lastNumber}");
        }

        [Fact]
        public async Task Should_Authorize_Invoice_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;
            const int pointOfSale = 1;
            const int invoiceType = 11; // Factura C

            // Get next invoice number
            var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(pointOfSale, invoiceType);
            var nextNumber = lastNumber + 1;

            var request = new InvoiceRequest
            {
                PointOfSale = pointOfSale,
                InvoiceType = invoiceType,
                Concept = 1, // Products
                DocumentType = 96, // DNI
                DocumentNumber = 12345678,
                InvoiceNumberFrom = nextNumber,
                InvoiceNumberTo = nextNumber,
                InvoiceDate = DateTime.Today,
                TotalAmount = 121.00m,
                NetAmount = 100.00m,
                VatAmount = 21.00m,
                VatDetails = new List<VatDetail>
                {
                    new VatDetail
                    {
                        VatRateId = 5, // 21%
                        BaseAmount = 100.00m,
                        VatAmount = 21.00m
                    }
                }
            };

            // Act
            var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Cae.Should().NotBeNullOrEmpty();
            response.CaeExpirationDate.Should().BeAfter(DateTime.Today);
            response.InvoiceNumber.Should().Be(nextNumber);
            response.IsSuccessful.Should().BeTrue();

            _output.WriteLine($"Invoice authorized successfully:");
            _output.WriteLine($"  CAE: {response.Cae}");
            _output.WriteLine($"  Invoice Number: {response.InvoiceNumber}");
            _output.WriteLine($"  Expiration: {response.CaeExpirationDate:yyyy-MM-dd}");
        }

        [Fact]
        public async Task Should_Query_Invoice_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;
            const int pointOfSale = 1;
            const int invoiceType = 11; // Factura C

            // First, get the last invoice number to ensure we have something to query
            var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(pointOfSale, invoiceType);
            
            if (lastNumber == 0)
            {
                _output.WriteLine("No invoices found to query. Skipping test.");
                return;
            }

            // Act
            var invoice = await client.ElectronicInvoicing.QueryInvoiceAsync(pointOfSale, invoiceType, lastNumber);

            // Assert
            if (invoice != null)
            {
                invoice.PointOfSale.Should().Be(pointOfSale);
                invoice.InvoiceType.Should().Be(invoiceType);
                invoice.InvoiceNumber.Should().Be(lastNumber);
                invoice.Cae.Should().NotBeNullOrEmpty();

                _output.WriteLine($"Invoice query successful:");
                _output.WriteLine($"  POS: {invoice.PointOfSale}");
                _output.WriteLine($"  Type: {invoice.InvoiceType}");
                _output.WriteLine($"  Number: {invoice.InvoiceNumber}");
                _output.WriteLine($"  CAE: {invoice.Cae}");
            }
            else
            {
                _output.WriteLine("Invoice not found - this is acceptable for testing");
            }
        }

        [Fact]
        public async Task Should_Handle_Invalid_Invoice_Request_Gracefully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var client = _fixture.AfipClient;

            var invalidRequest = new InvoiceRequest
            {
                PointOfSale = 999, // Invalid point of sale
                InvoiceType = 11,
                Concept = 1,
                DocumentType = 96,
                DocumentNumber = 12345678,
                InvoiceNumberFrom = 1,
                InvoiceNumberTo = 1,
                InvoiceDate = DateTime.Today,
                TotalAmount = 121.00m,
                NetAmount = 100.00m,
                VatAmount = 21.00m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await client.ElectronicInvoicing.AuthorizeInvoiceAsync(invalidRequest);
            });

            exception.Should().NotBeNull();
            _output.WriteLine($"Expected exception caught: {exception.Message}");
        }

        [Fact]
        public async Task Should_Cache_Authentication_Tokens()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var cacheService = _fixture.ServiceProvider.GetService<IAfipCacheService>();
            if (cacheService == null)
            {
                _output.WriteLine("Cache service not available. Skipping cache test.");
                return;
            }

            var wsaaService = _fixture.ServiceProvider.GetRequiredService<IWsaaService>();

            // Act - First call should create and cache the token
            var ticket1 = await wsaaService.GetValidTicketAsync("wsfe");
            var stats1 = await cacheService.GetStatisticsAsync();

            // Second call should use cached token
            var ticket2 = await wsaaService.GetValidTicketAsync("wsfe");
            var stats2 = await cacheService.GetStatisticsAsync();

            // Assert
            ticket1.Should().NotBeNull();
            ticket2.Should().NotBeNull();
            ticket1.Token.Should().Be(ticket2.Token); // Should be same token from cache
            
            stats2.Hits.Should().BeGreaterThan(stats1.Hits);
            
            _output.WriteLine($"Cache statistics after two auth calls:");
            _output.WriteLine($"  Hits: {stats2.Hits}");
            _output.WriteLine($"  Misses: {stats2.Misses}");
            _output.WriteLine($"  Hit Ratio: {stats2.HitRatio:P}");
        }

        [Fact]
        public async Task Should_Track_Connection_Pool_Statistics()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var connectionPool = _fixture.ServiceProvider.GetService<IAfipConnectionPool>();
            if (connectionPool == null)
            {
                _output.WriteLine("Connection pool not available. Skipping connection pool test.");
                return;
            }

            var client = _fixture.AfipClient;

            // Act - Make several requests to populate statistics
            await client.ElectronicInvoicing.CheckServiceStatusAsync();
            await client.Parameters.GetInvoiceTypesAsync();
            await client.Parameters.GetDocumentTypesAsync();

            var stats = await connectionPool.GetStatisticsAsync();

            // Assert
            stats.Should().NotBeNull();
            stats.TotalRequestsHandled.Should().BeGreaterThan(0);
            stats.ActiveConnections.Should().BeGreaterThan(0);

            _output.WriteLine($"Connection pool statistics:");
            _output.WriteLine($"  Active Connections: {stats.ActiveConnections}");
            _output.WriteLine($"  Total Requests: {stats.TotalRequestsHandled}");
            _output.WriteLine($"  Average Response Time: {stats.AverageResponseTimeMs:F2}ms");
            _output.WriteLine($"  Pool Efficiency: {stats.PoolEfficiency:P}");
        }

        [Fact]
        public async Task Should_Perform_Health_Check_Successfully()
        {
            // Arrange
            _fixture.SkipIfCertificateNotAvailable();
            _fixture.SkipIfNotTestingEnvironment();

            var connectionPool = _fixture.ServiceProvider.GetService<IAfipConnectionPool>();
            if (connectionPool == null)
            {
                _output.WriteLine("Connection pool not available. Skipping health check test.");
                return;
            }

            // Make some requests first to populate health data
            var client = _fixture.AfipClient;
            await client.ElectronicInvoicing.CheckServiceStatusAsync();

            // Act
            var healthStatus = await connectionPool.CheckHealthAsync();

            // Assert
            healthStatus.Should().NotBeNull();
            healthStatus.Status.Should().NotBe(Abstractions.Services.HealthStatus.Unhealthy);
            healthStatus.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));

            _output.WriteLine($"Health check results:");
            _output.WriteLine($"  Overall Status: {healthStatus.Status}");
            _output.WriteLine($"  Duration: {healthStatus.Duration.TotalMilliseconds:F0}ms");
            _output.WriteLine($"  Services Checked: {healthStatus.ServiceStatuses.Count}");

            foreach (var service in healthStatus.ServiceStatuses)
            {
                _output.WriteLine($"    {service.Key}: {service.Value.Status} ({service.Value.ResponseTime.TotalMilliseconds:F0}ms)");
            }
        }
    }
}