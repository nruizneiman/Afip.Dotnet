using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Afip.Dotnet.DependencyInjection.Extensions;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;

namespace Afip.Dotnet.Examples.DependencyInjection;

/// <summary>
/// Example demonstrating how to use AFIP SDK with dependency injection
/// in a console application using the Microsoft.Extensions.Hosting pattern
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("AFIP .NET SDK - Dependency Injection Example");
        Console.WriteLine("============================================");

        // Create host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure AFIP services for testing environment
                services.AddAfipServicesForTesting(
                    cuit: 20123456789, // Replace with your CUIT
                    certificatePath: "certificates/testing.p12", // Replace with your certificate path
                    certificatePassword: "test-password" // Replace with your certificate password
                );

                // Register our example services
                services.AddScoped<IInvoiceService, InvoiceService>();
                services.AddScoped<IParameterService, ParameterService>();
                services.AddScoped<ExampleOrchestrator>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        try
        {
            // Run the example
            using var scope = host.Services.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<ExampleOrchestrator>();
            await orchestrator.RunExampleAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure you have:");
            Console.WriteLine("1. Valid CUIT number");
            Console.WriteLine("2. Valid certificate file path");
            Console.WriteLine("3. Correct certificate password");
            Console.WriteLine("4. Internet connection for AFIP services");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}

/// <summary>
/// Service interface for invoice operations
/// </summary>
public interface IInvoiceService
{
    Task<InvoiceResponse> CreateInvoiceAsync(decimal amount, string customerDocument);
    Task<long> GetNextInvoiceNumberAsync();
    Task<InvoiceResponse?> GetInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber);
}

/// <summary>
/// Service interface for parameter operations
/// </summary>
public interface IParameterService
{
    Task DisplayServiceStatusAsync();
    Task DisplayParameterTablesAsync();
}

/// <summary>
/// Invoice service implementation using dependency injection
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly IAfipClient _afipClient;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IAfipClient afipClient, ILogger<InvoiceService> logger)
    {
        _afipClient = afipClient;
        _logger = logger;
    }

    public async Task<InvoiceResponse> CreateInvoiceAsync(decimal amount, string customerDocument)
    {
        _logger.LogInformation("Creating invoice for amount {Amount} and customer {Customer}", 
            amount, customerDocument);

        try
        {
            // Get next invoice number
            var nextNumber = await GetNextInvoiceNumberAsync();

            // Calculate VAT (21%)
            var vatRate = 0.21m;
            var netAmount = amount / (1 + vatRate);
            var vatAmount = amount - netAmount;

            // Create invoice request
            var request = new InvoiceRequest
            {
                PointOfSale = 1,
                InvoiceType = 11, // Invoice C
                Concept = 1, // Products
                DocumentType = 96, // DNI
                DocumentNumber = long.Parse(customerDocument),
                InvoiceNumberFrom = nextNumber,
                InvoiceNumberTo = nextNumber,
                InvoiceDate = DateTime.Today,
                TotalAmount = amount,
                NetAmount = netAmount,
                VatAmount = vatAmount,
                VatDetails = new List<VatDetail>
                {
                    new VatDetail
                    {
                        VatRateId = 5, // 21%
                        BaseAmount = netAmount,
                        VatAmount = vatAmount
                    }
                }
            };

            // Authorize invoice
            var response = await _afipClient.ElectronicInvoicing.AuthorizeInvoiceAsync(request);

            _logger.LogInformation("Invoice authorized with CAE: {Cae}, Expiration: {Expiration}", 
                response.Cae, response.CaeExpirationDate);

            return response;
        }
        catch (AfipException ex)
        {
            _logger.LogError(ex, "AFIP error creating invoice: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<long> GetNextInvoiceNumberAsync()
    {
        var lastNumber = await _afipClient.ElectronicInvoicing
            .GetLastInvoiceNumberAsync(pointOfSale: 1, invoiceType: 11);
        
        return lastNumber + 1;
    }

    public async Task<InvoiceResponse?> GetInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber)
    {
        try
        {
            return await _afipClient.ElectronicInvoicing
                .QueryInvoiceAsync(pointOfSale, invoiceType, invoiceNumber);
        }
        catch (AfipException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Invoice not found: POS {PointOfSale}, Type {InvoiceType}, Number {InvoiceNumber}",
                pointOfSale, invoiceType, invoiceNumber);
            return null;
        }
    }
}

/// <summary>
/// Parameter service implementation using dependency injection
/// </summary>
public class ParameterService : IParameterService
{
    private readonly IAfipClient _afipClient;
    private readonly ILogger<ParameterService> _logger;

    public ParameterService(IAfipClient afipClient, ILogger<ParameterService> logger)
    {
        _afipClient = afipClient;
        _logger = logger;
    }

    public async Task DisplayServiceStatusAsync()
    {
        _logger.LogInformation("Checking AFIP service status...");

        try
        {
            var status = await _afipClient.ElectronicInvoicing.CheckServiceStatusAsync();
            
            Console.WriteLine("\n=== AFIP Service Status ===");
            Console.WriteLine($"Application: {status.AppServer}");
            Console.WriteLine($"Authentication: {status.AuthServer}");
            Console.WriteLine($"Database: {status.DbServer}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service status");
            Console.WriteLine("Service status check failed - AFIP services may be unavailable");
        }
    }

    public async Task DisplayParameterTablesAsync()
    {
        _logger.LogInformation("Retrieving AFIP parameter tables...");

        try
        {
            Console.WriteLine("\n=== AFIP Parameter Tables ===");

            // Get invoice types
            var invoiceTypes = await _afipClient.Parameters.GetInvoiceTypesAsync();
            Console.WriteLine($"\nInvoice Types ({invoiceTypes.Count} found):");
            foreach (var type in invoiceTypes.Take(5)) // Show first 5
            {
                Console.WriteLine($"  {type.Id}: {type.Description}");
            }
            if (invoiceTypes.Count > 5)
                Console.WriteLine($"  ... and {invoiceTypes.Count - 5} more");

            // Get document types
            var documentTypes = await _afipClient.Parameters.GetDocumentTypesAsync();
            Console.WriteLine($"\nDocument Types ({documentTypes.Count} found):");
            foreach (var type in documentTypes.Take(5)) // Show first 5
            {
                Console.WriteLine($"  {type.Id}: {type.Description}");
            }
            if (documentTypes.Count > 5)
                Console.WriteLine($"  ... and {documentTypes.Count - 5} more");

            // Get VAT rates
            var vatRates = await _afipClient.Parameters.GetVatRatesAsync();
            Console.WriteLine($"\nVAT Rates ({vatRates.Count} found):");
            foreach (var rate in vatRates)
            {
                Console.WriteLine($"  {rate.Id}: {rate.Description}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parameter tables");
            Console.WriteLine("Parameter tables retrieval failed");
        }
    }
}

/// <summary>
/// Main orchestrator that demonstrates various SDK features
/// </summary>
public class ExampleOrchestrator
{
    private readonly IInvoiceService _invoiceService;
    private readonly IParameterService _parameterService;
    private readonly ILogger<ExampleOrchestrator> _logger;

    public ExampleOrchestrator(
        IInvoiceService invoiceService,
        IParameterService parameterService,
        ILogger<ExampleOrchestrator> logger)
    {
        _invoiceService = invoiceService;
        _parameterService = parameterService;
        _logger = logger;
    }

    public async Task RunExampleAsync()
    {
        _logger.LogInformation("Starting AFIP SDK example with dependency injection");

        // 1. Check service status
        await _parameterService.DisplayServiceStatusAsync();

        // 2. Display parameter tables
        await _parameterService.DisplayParameterTablesAsync();

        // 3. Get next invoice number
        Console.WriteLine("\n=== Invoice Operations ===");
        var nextNumber = await _invoiceService.GetNextInvoiceNumberAsync();
        Console.WriteLine($"Next invoice number: {nextNumber}");

        // 4. Create a sample invoice
        Console.WriteLine("\nCreating sample invoice...");
        var invoiceResponse = await _invoiceService.CreateInvoiceAsync(
            amount: 121.00m,
            customerDocument: "12345678"
        );

        Console.WriteLine($"✅ Invoice created successfully!");
        Console.WriteLine($"   CAE: {invoiceResponse.Cae}");
        Console.WriteLine($"   Invoice Number: {invoiceResponse.InvoiceNumber}");
        Console.WriteLine($"   Expiration Date: {invoiceResponse.CaeExpirationDate:yyyy-MM-dd}");

        // 5. Retrieve the created invoice
        Console.WriteLine("\nRetrieving created invoice...");
        var retrievedInvoice = await _invoiceService.GetInvoiceAsync(
            pointOfSale: 1,
            invoiceType: 11,
            invoiceNumber: invoiceResponse.InvoiceNumber
        );

        if (retrievedInvoice != null)
        {
            Console.WriteLine($"✅ Invoice retrieved successfully!");
            Console.WriteLine($"   Total Amount: ${retrievedInvoice.TotalAmount:F2}");
            Console.WriteLine($"   Net Amount: ${retrievedInvoice.NetAmount:F2}");
            Console.WriteLine($"   VAT Amount: ${retrievedInvoice.VatAmount:F2}");
        }
        else
        {
            Console.WriteLine("❌ Invoice not found");
        }

        _logger.LogInformation("Example completed successfully");
    }
}