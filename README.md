# AFIP .NET SDK

[![CI/CD Pipeline](https://github.com/nruizneiman/Afip.Dotnet/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/nruizneiman/Afip.Dotnet/actions/workflows/ci-cd.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Afip.Dotnet.svg)](https://www.nuget.org/packages/Afip.Dotnet/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Afip.Dotnet.svg)](https://www.nuget.org/packages/Afip.Dotnet/)
[![Coverage Status](https://codecov.io/gh/nruizneiman/Afip.Dotnet/branch/main/graph/badge.svg)](https://codecov.io/gh/nruizneiman/Afip.Dotnet)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive and modern .NET SDK for integrating with AFIP (now ARCA - Agencia de Recaudación y Control Aduanero) web services. This SDK provides a clean, async/await-based API for electronic invoicing, authentication, and parameter management.

## 🚀 Features

- **🔐 WSAA Authentication**: Automatic ticket management with certificate-based authentication
- **🧾 Electronic Invoicing (WSFEv1)**: Complete support for invoice authorization and management
- **💰 Foreign Currency Support**: Full compliance with RG 5616/2024 (FEv4)
- **📊 Parameter Tables**: Dynamic access to AFIP parameter tables
- **⚡ Async/Await**: Modern asynchronous programming patterns
- **🛡️ Type Safety**: Strongly-typed models for all AFIP operations
- **📝 Comprehensive Logging**: Built-in logging support with Microsoft.Extensions.Logging
- **🧪 Testing Support**: Separate testing and production environments
- **📦 NuGet Packages**: Easy installation via NuGet
- **🔧 Dependency Injection**: Separate DI package for clean architecture

### Supported AFIP Services

| Service | Description | Status |
|---------|-------------|--------|
| **WSAA** | Web Service Authentication and Authorization | ✅ Complete |
| **WSFEv1** | Electronic Invoicing for domestic market | ✅ Complete |
| **WSFEX** | Electronic Invoicing for exports | ✅ Complete |
| **WSMTXCA** | Electronic Invoicing with item details | ✅ Complete |

### Supported Invoice Types

- **Invoice A, B, C** - Standard domestic invoices
- **FCE MiPyMEs** - Credit invoices for small and medium enterprises (RG 4367/2018)
- **Foreign Currency Invoices** - USD and other currencies (RG 5616/2024)
- **Credit and Debit Notes** - With invoice associations
- **Export Invoices (E)** - For international transactions

### Regulatory Compliance

This SDK implements and supports the following AFIP regulations:

- **RG 2485, RG 2904, RG 3067** - Base electronic invoicing regulations
- **RG 3668/2014** - Special regimes for restaurants, hotels, and bars
- **RG 3749/2015** - VAT responsible parties and exempt entities
- **RG 4367/2018** - FCE MiPyMEs credit invoices
- **RG 5616/2024** - Foreign currency operations (FEv4)

## 📦 Installation

### Core Package

```bash
dotnet add package Afip.Dotnet
```

### Dependency Injection Package (Optional)

```bash
dotnet add package Afip.Dotnet.DependencyInjection
```

### PackageReference

```xml
<PackageReference Include="Afip.Dotnet" Version="1.0.0" />
<PackageReference Include="Afip.Dotnet.DependencyInjection" Version="1.0.0" />
```

## ⚙️ Configuration

### 1. Certificate Setup

First, you need an X.509 certificate from AFIP in PKCS#12 format (.p12 or .pfx):

```bash
# Generate private key
openssl genrsa -out private.key 2048

# Generate certificate request
openssl req -new -key private.key -out certificate.csr -subj "/C=AR/O=YourCompany/CN=YourName/serialNumber=CUIT YourCuit"

# After AFIP approves, convert to PKCS#12
openssl pkcs12 -export -out certificate.p12 -inkey private.key -in certificate.crt
```

### 2. Basic Configuration

```csharp
using Afip.Dotnet;
using Afip.Dotnet.Abstractions.Models;

var configuration = new AfipConfiguration
{
    Environment = AfipEnvironment.Testing, // or AfipEnvironment.Production
    Cuit = 20123456789,
    CertificatePath = "path/to/your/certificate.p12",
    CertificatePassword = "your-certificate-password",
    TimeoutSeconds = 30
};

using var client = AfipClient.Create(configuration);
```

### 3. Factory Methods

For quick setup:

```csharp
// Testing environment
using var testingClient = AfipClient.CreateForTesting(
    cuit: 20123456789,
    certificatePath: "cert.p12",
    certificatePassword: "password"
);

// Production environment
using var productionClient = AfipClient.CreateForProduction(
    cuit: 20123456789,
    certificatePath: "cert.p12",
    certificatePassword: "password"
);
```

## 🔧 Usage Examples

### Basic Service Status Check

```csharp
// Check if the service is available
var status = await client.ElectronicInvoicing.CheckServiceStatusAsync();
Console.WriteLine($"Service Status: {status.AppServer} - {status.DbServer} - {status.AuthServer}");
```

### Get Next Invoice Number

```csharp
// Get the next available invoice number
var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(
    pointOfSale: 1,
    invoiceType: 11 // Invoice C
);
Console.WriteLine($"Next invoice number: {lastNumber + 1}");
```

### Authorize a Simple Invoice

```csharp
var invoiceRequest = new InvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 11, // Invoice C
    Concept = 1, // Products
    DocumentType = 96, // DNI
    DocumentNumber = 12345678,
    InvoiceNumberFrom = 1,
    InvoiceNumberTo = 1,
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

var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(invoiceRequest);
Console.WriteLine($"CAE: {response.Cae}, Expiration: {response.CaeExpirationDate}");
```

### Foreign Currency Invoice (USD)

```csharp
var usdInvoice = new InvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 19, // Invoice E (Export)
    Concept = 1,
    DocumentType = 80, // CUIT
    DocumentNumber = 20987654321,
    InvoiceNumberFrom = 1,
    InvoiceNumberTo = 1,
    InvoiceDate = DateTime.Today,
    CurrencyId = "DOL", // USD
    CurrencyRate = 350.50m, // Exchange rate
    TotalAmount = 1000.00m,
    NetAmount = 1000.00m,
    PayInSameForeignCurrency = true,
    ReceiverVatCondition = 4 // Foreign consumer
};

var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(usdInvoice);
```

### Export Invoice (WSFEX)

```csharp
var exportInvoice = new ExportInvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 19, // Invoice E (Export)
    InvoiceNumberFrom = 1,
    InvoiceNumberTo = 1,
    InvoiceDate = DateTime.Today,
    DocumentType = 80, // CUIT
    DocumentNumber = 20987654321,
    ReceiverName = "International Buyer Corp",
    ReceiverAddress = "123 Main St",
    ReceiverCity = "New York",
    ReceiverCountryCode = "US",
    CurrencyId = "DOL", // USD
    CurrencyRate = 350.50m,
    TotalAmount = 1000.00m,
    NetAmount = 1000.00m,
    VatAmount = 0.00m,
    ExportDestination = 3, // Other countries
    Incoterm = 3, // FOB
    Language = 2 // English
};

var exportResponse = await client.ExportInvoicing.AuthorizeExportInvoiceAsync(exportInvoice);
Console.WriteLine($"Export CAE: {exportResponse.Cae}");
```

### Detailed Invoice with Items (WSMTXCA)

```csharp
var detailedInvoice = new DetailedInvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 11, // Invoice C
    InvoiceNumberFrom = 1,
    InvoiceNumberTo = 1,
    InvoiceDate = DateTime.Today,
    DocumentType = 96, // DNI
    DocumentNumber = 12345678,
    ReceiverName = "Juan Pérez",
    ReceiverAddress = "Av. Corrientes 123",
    ReceiverCity = "Buenos Aires",
    ReceiverPostalCode = "1043",
    ReceiverVatCondition = 5, // Consumidor Final
    TotalAmount = 242.00m,
    NetAmount = 200.00m,
    VatAmount = 42.00m,
    Items = new List<InvoiceItem>
    {
        new InvoiceItem
        {
            Description = "Laptop HP Pavilion",
            Quantity = 1,
            UnitPrice = 200.00m,
            TotalAmount = 242.00m,
            NetAmount = 200.00m,
            VatAmount = 42.00m,
            UnitOfMeasurement = 6, // Unit
            ItemCategory = 1, // Products
            ItemType = 1, // Goods
            VatRate = 5 // 21%
        }
    },
    VatDetails = new List<VatDetail>
    {
        new VatDetail
        {
            VatRateId = 5, // 21%
            BaseAmount = 200.00m,
            VatAmount = 42.00m
        }
    }
};

var detailedResponse = await client.DetailedInvoicing.AuthorizeDetailedInvoiceAsync(detailedInvoice);
Console.WriteLine($"Detailed Invoice CAE: {detailedResponse.Cae}");
```

### Credit Note with Association

```csharp
var creditNote = new InvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 213, // Credit Note C
    Concept = 1,
    DocumentType = 96,
    DocumentNumber = 12345678,
    InvoiceNumberFrom = 1,
    InvoiceNumberTo = 1,
    InvoiceDate = DateTime.Today,
    TotalAmount = 50.00m,
    NetAmount = 41.32m,
    VatAmount = 8.68m,
    
    // Associate with original invoice
    AssociatedInvoices = new List<AssociatedInvoice>
    {
        new AssociatedInvoice
        {
            InvoiceType = 11,
            PointOfSale = 1,
            InvoiceNumber = 123L
        }
    },
    
    VatDetails = new List<VatDetail>
    {
        new VatDetail
        {
            VatRateId = 5,
            BaseAmount = 41.32m,
            VatAmount = 8.68m
        }
    }
};

var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(creditNote);
```

### Query Existing Invoice

```csharp
var invoice = await client.ElectronicInvoicing.QueryInvoiceAsync(
    pointOfSale: 1,
    invoiceType: 11,
    invoiceNumber: 123
);

Console.WriteLine($"Invoice Amount: {invoice.TotalAmount}");
Console.WriteLine($"CAE: {invoice.Cae}");
Console.WriteLine($"Authorized: {invoice.ProcessedDate}");
```

### Batch Invoice Processing

```csharp
var batchRequests = new List<InvoiceRequest>
{
    // Create multiple invoice requests...
    invoice1,
    invoice2,
    invoice3
};

var responses = new List<InvoiceResponse>();

foreach (var request in batchRequests)
{
    try
    {
        var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
        responses.Add(response);
        Console.WriteLine($"Invoice {request.InvoiceNumberFrom} authorized with CAE: {response.Cae}");
    }
    catch (AfipException ex)
    {
        Console.WriteLine($"Error authorizing invoice {request.InvoiceNumberFrom}: {ex.Message}");
    }
}
```

### Working with Parameter Tables

```csharp
// Get invoice types
var invoiceTypes = await client.Parameters.GetInvoiceTypesAsync();
foreach (var type in invoiceTypes)
{
    Console.WriteLine($"{type.Id}: {type.Description}");
}

// Get currencies
var currencies = await client.Parameters.GetCurrenciesAsync();
var usd = currencies.FirstOrDefault(c => c.Id == "DOL");
Console.WriteLine($"USD Rate: {usd?.Rate}");

// Get VAT rates
var vatRates = await client.Parameters.GetVatRatesAsync();
foreach (var rate in vatRates)
{
    Console.WriteLine($"{rate.Id}: {rate.Description} - {rate.Percentage}%");
}

// Get document types
var documentTypes = await client.Parameters.GetDocumentTypesAsync();
var cuit = documentTypes.FirstOrDefault(d => d.Id == "80");
Console.WriteLine($"CUIT: {cuit?.Description}");
```

## 🔒 Authentication Management

The SDK automatically handles WSAA authentication:

```csharp
// Authentication is handled automatically, but you can access it directly
var ticket = await client.Authentication.GetValidTicketAsync("wsfe");
Console.WriteLine($"Token expires at: {ticket.ExpiresAt}");

// Clear ticket cache
client.Authentication.ClearTicketCache();

// Check ticket status
var isValid = ticket.IsValid;
var willExpireSoon = ticket.WillExpireSoon(10); // Check if expires in 10 minutes
```

## 🚨 Error Handling

```csharp
try
{
    var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
}
catch (AfipException ex)
{
    Console.WriteLine($"AFIP Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network Error: {ex.Message}");
}
catch (TaskCanceledException ex)
{
    Console.WriteLine($"Request Timeout: {ex.Message}");
}
```

## 📝 Logging

The SDK supports Microsoft.Extensions.Logging:

```csharp
using Microsoft.Extensions.Logging;

// Configure logging
var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<AfipClient>();

// Create client with logging
var config = new AfipConfiguration
{
    // ... configuration
    EnableLogging = true
};

using var client = new AfipClient(config, logger);
```

## 🧪 Testing

The SDK includes comprehensive unit tests and supports both testing and production environments:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Unit
```

### Test Configuration

```csharp
// Use AFIP's testing environment
var testConfig = new AfipConfiguration
{
    Environment = AfipEnvironment.Testing,
    Cuit = 20123456789, // Use AFIP's test CUIT
    CertificatePath = "test-certificate.p12",
    CertificatePassword = "test-password"
};
```

## 📊 Performance Considerations

- **Connection Pooling**: The SDK reuses HTTP connections
- **Ticket Caching**: Authentication tickets are cached and automatically renewed
- **Async Operations**: All operations are asynchronous for better scalability
- **Memory Efficiency**: Minimal memory footprint with proper disposal patterns

## 🔄 Migration from Other Libraries

If you're migrating from other AFIP libraries:

```csharp
// Old library pattern
var oldClient = new SomeAfipLibrary();
var result = oldClient.AutorizarComprobante(data);

// New SDK pattern
using var newClient = AfipClient.CreateForProduction(cuit, certPath, certPassword);
var response = await newClient.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
```

## 🛠️ Advanced Configuration

### Custom HTTP Client

```csharp
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");

var config = new AfipConfiguration
{
    // ... other settings
    CustomHttpClient = httpClient
};
```

### Custom URLs (for testing)

```csharp
var config = new AfipConfiguration
{
    Environment = AfipEnvironment.Testing,
    CustomWsaaUrl = "https://custom-wsaa-url",
    CustomWsfev1Url = "https://custom-wsfev1-url",
    // ... other settings
};
```

## 📋 Requirements

- **.NET Standard 2.0** or higher
- **.NET Framework 4.6.1** or higher
- **.NET Core 2.0** or higher
- **.NET 5.0** or higher

### Dependencies

- `System.ServiceModel.Http` (>= 4.10.0)
- `System.Security.Cryptography.Pkcs` (>= 6.0.0)
- `Microsoft.Extensions.Logging.Abstractions` (>= 6.0.0)

## 🔧 Dependency Injection

The AFIP SDK provides seamless integration with Microsoft.Extensions.DependencyInjection through a separate package, making it easy to use in ASP.NET Core, Worker Services, and other .NET applications.

### Installation

First, install the AFIP SDK DI package:

```bash
dotnet add package Afip.Dotnet.DependencyInjection
```

### Basic Registration

```csharp
using Afip.Dotnet.DependencyInjection.Extensions;
using Afip.Dotnet.Abstractions.Models;

// Register AFIP services with configuration
services.AddAfipServices(new AfipConfiguration
{
    Environment = AfipEnvironment.Testing,
    Cuit = 20123456789,
    CertificatePath = "path/to/certificate.p12",
    CertificatePassword = "certificate-password",
    EnableLogging = true
});
```

### Configuration-based Registration

```csharp
// Register with action-based configuration
services.AddAfipServices(config =>
{
    config.Environment = AfipEnvironment.Production;
    config.Cuit = 20123456789;
    config.CertificatePath = "certificates/production.p12";
    config.CertificatePassword = Environment.GetEnvironmentVariable("AFIP_CERT_PASSWORD");
    config.TimeoutSeconds = 60;
    config.EnableLogging = true;
});
```

### Quick Setup Methods

```csharp
// For testing environment
services.AddAfipServicesForTesting(
    cuit: 20123456789,
    certificatePath: "test-cert.p12", 
    certificatePassword: "test-password"
);

// For production environment
services.AddAfipServicesForProduction(
    cuit: 20123456789,
    certificatePath: "prod-cert.p12",
    certificatePassword: Environment.GetEnvironmentVariable("CERT_PASSWORD")!
);
```

### Optimized Registration

```csharp
// Register with all optimizations enabled
services.AddAfipServicesOptimized(new AfipConfiguration
{
    Environment = AfipEnvironment.Production,
    Cuit = 20123456789,
    CertificatePath = "cert.p12",
    CertificatePassword = "password"
});

// Register with minimal dependencies
services.AddAfipServicesMinimal(new AfipConfiguration
{
    Environment = AfipEnvironment.Testing,
    Cuit = 20123456789,
    CertificatePath = "test-cert.p12",
    CertificatePassword = "test-password"
});
```

## 📱 Usage Examples with Dependency Injection

### ASP.NET Core Web API

```csharp
// Program.cs (.NET 6+)
using Afip.Dotnet.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add AFIP services
builder.Services.AddAfipServicesForTesting(
    cuit: 20123456789,
    certificatePath: "certificates/testing.p12",
    certificatePassword: builder.Configuration["Afip:CertificatePassword"]!
);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();

// Controllers/InvoiceController.cs
using Microsoft.AspNetCore.Mvc;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Abstractions.Models.Invoice;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IAfipClient _afipClient;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(IAfipClient afipClient, ILogger<InvoiceController> logger)
    {
        _afipClient = afipClient;
        _logger = logger;
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> AuthorizeInvoice([FromBody] InvoiceRequest request)
    {
        try
        {
            var response = await _afipClient.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
            
            _logger.LogInformation("Invoice authorized with CAE: {Cae}", response.Cae);
            
            return Ok(new
            {
                Success = true,
                Cae = response.Cae,
                ExpirationDate = response.CaeExpirationDate,
                InvoiceNumber = request.InvoiceNumberFrom
            });
        }
        catch (AfipException ex)
        {
            _logger.LogError(ex, "AFIP error while authorizing invoice");
            return BadRequest(new { Error = ex.Message, Code = ex.ErrorCode });
        }
    }

    [HttpGet("next-number/{pointOfSale}/{invoiceType}")]
    public async Task<IActionResult> GetNextInvoiceNumber(int pointOfSale, int invoiceType)
    {
        try
        {
            var lastNumber = await _afipClient.ElectronicInvoicing.GetLastInvoiceNumberAsync(
                pointOfSale, invoiceType);
            
            return Ok(new { NextNumber = lastNumber + 1 });
        }
        catch (AfipException ex)
        {
            _logger.LogError(ex, "Error getting next invoice number");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetServiceStatus()
    {
        try
        {
            var status = await _afipClient.ElectronicInvoicing.CheckServiceStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service status");
            return StatusCode(500, new { Error = "Service unavailable" });
        }
    }
}
```

### Worker Service Background Processing

```csharp
// Program.cs
using Afip.Dotnet.DependencyInjection.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Add AFIP services
builder.Services.AddAfipServicesForProduction(
    cuit: long.Parse(builder.Configuration["Afip:Cuit"]!),
    certificatePath: builder.Configuration["Afip:CertificatePath"]!,
    certificatePassword: builder.Configuration["Afip:CertificatePassword"]!
);

builder.Services.AddHostedService<InvoiceProcessorWorker>();

var host = builder.Build();
host.Run();

// InvoiceProcessorWorker.cs
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Abstractions.Models.Invoice;

public class InvoiceProcessorWorker : BackgroundService
{
    private readonly IAfipClient _afipClient;
    private readonly ILogger<InvoiceProcessorWorker> _logger;

    public InvoiceProcessorWorker(IAfipClient afipClient, ILogger<InvoiceProcessorWorker> logger)
    {
        _afipClient = afipClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Process pending invoices from database/queue
                var pendingInvoices = await GetPendingInvoicesFromDatabase();

                foreach (var invoiceData in pendingInvoices)
                {
                    var request = MapToInvoiceRequest(invoiceData);
                    
                    var response = await _afipClient.ElectronicInvoicing
                        .AuthorizeInvoiceAsync(request, stoppingToken);
                    
                    await UpdateInvoiceInDatabase(invoiceData.Id, response);
                    
                    _logger.LogInformation("Processed invoice {InvoiceId} with CAE {Cae}", 
                        invoiceData.Id, response.Cae);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoices");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task<List<InvoiceData>> GetPendingInvoicesFromDatabase()
    {
        // Implementation to get pending invoices
        return new List<InvoiceData>();
    }

    private InvoiceRequest MapToInvoiceRequest(InvoiceData data)
    {
        // Implementation to map from your data model to InvoiceRequest
        return new InvoiceRequest();
    }

    private async Task UpdateInvoiceInDatabase(int invoiceId, InvoiceResponse response)
    {
        // Implementation to update invoice with AFIP response
    }
}
```

### Console Application

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Afip.Dotnet.DependencyInjection.Extensions;
using Afip.Dotnet.Abstractions.Services;
using Afip.Dotnet.Abstractions.Models.Invoice;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add AFIP services
        services.AddAfipServicesForTesting(
            cuit: 20123456789,
            certificatePath: "certificates/testing.p12",
            certificatePassword: "test-password"
        );
        
        services.AddScoped<InvoiceProcessor>();
    })
    .Build();

// Run the application
using var scope = host.Services.CreateScope();
var processor = scope.ServiceProvider.GetRequiredService<InvoiceProcessor>();
await processor.ProcessInvoiceAsync();

public class InvoiceProcessor
{
    private readonly IAfipClient _afipClient;
    private readonly ILogger<InvoiceProcessor> _logger;

    public InvoiceProcessor(IAfipClient afipClient, ILogger<InvoiceProcessor> logger)
    {
        _afipClient = afipClient;
        _logger = logger;
    }

    public async Task ProcessInvoiceAsync()
    {
        try
        {
            // Check service status
            var status = await _afipClient.ElectronicInvoicing.CheckServiceStatusAsync();
            _logger.LogInformation("AFIP Service Status: {Status}", status.AppServer);

            // Get next invoice number
            var lastNumber = await _afipClient.ElectronicInvoicing
                .GetLastInvoiceNumberAsync(pointOfSale: 1, invoiceType: 11);
            var nextNumber = lastNumber + 1;

            // Create and authorize invoice
            var request = new InvoiceRequest
            {
                PointOfSale = 1,
                InvoiceType = 11, // Invoice C
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

            var response = await _afipClient.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
            
            _logger.LogInformation("Invoice authorized successfully!");
            _logger.LogInformation("CAE: {Cae}", response.Cae);
            _logger.LogInformation("Expiration: {ExpirationDate}", response.CaeExpirationDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice");
        }
    }
}
```

### Configuration with appsettings.json

```json
// appsettings.json
{
  "Afip": {
    "Environment": "Testing",
    "Cuit": "20123456789",
    "CertificatePath": "certificates/testing.p12",
    "CertificatePassword": "your-certificate-password",
    "TimeoutSeconds": 30,
    "EnableLogging": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Afip.Dotnet": "Debug"
    }
  }
}
```

```csharp
// Configuration binding
services.AddAfipServices(config =>
{
    var afipSettings = builder.Configuration.GetSection("Afip");
    config.Environment = Enum.Parse<AfipEnvironment>(afipSettings["Environment"]!);
    config.Cuit = long.Parse(afipSettings["Cuit"]!);
    config.CertificatePath = afipSettings["CertificatePath"]!;
    config.CertificatePassword = afipSettings["CertificatePassword"]!;
    config.TimeoutSeconds = afipSettings.GetValue<int>("TimeoutSeconds");
    config.EnableLogging = afipSettings.GetValue<bool>("EnableLogging");
});
```

### Advanced Scenarios

#### Custom Service Registration

```csharp
// Register with custom lifetimes
services.AddAfipServices(config => { /* configuration */ });

// Override specific services if needed
services.AddSingleton<IAfipClient, CustomAfipClient>();
```

#### Multiple AFIP Configurations

```csharp
// For applications handling multiple companies
services.AddKeyedScoped<IAfipClient>("Company1", (provider, key) =>
{
    var config = new AfipConfiguration
    {
        Cuit = 20111111111,
        CertificatePath = "certs/company1.p12",
        // ... other settings
    };
    return new AfipClient(config, provider.GetService<ILoggerFactory>());
});

services.AddKeyedScoped<IAfipClient>("Company2", (provider, key) =>
{
    var config = new AfipConfiguration
    {
        Cuit = 20222222222,
        CertificatePath = "certs/company2.p12",
        // ... other settings
    };
    return new AfipClient(config, provider.GetService<ILoggerFactory>());
});

// Usage in controllers/services
public class MultiCompanyInvoiceController : ControllerBase
{
    public async Task<IActionResult> AuthorizeForCompany1(
        [FromKeyedServices("Company1")] IAfipClient afipClient,
        [FromBody] InvoiceRequest request)
    {
        var response = await afipClient.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
        return Ok(response);
    }
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 6.0 SDK or later
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet test`

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Related Links

### Official AFIP Documentation
- [AFIP Official Portal](https://www.afip.gob.ar/)
- [ARCA Official Site](https://www.argentina.gob.ar/arca)
- [Electronic Invoicing Main Page](https://www.afip.gob.ar/facturae/)
- [Web Services Documentation](https://www.afip.gob.ar/ws/)

### Technical Documentation
- [WSFEv1 Developer Manual](https://www.afip.gob.ar/ws/WSFE/WSFE-manual_desarrollador.pdf)
- [WSAA Authentication Manual](https://www.afip.gob.ar/ws/WSAA/wsaa_manual_desarrollador.pdf)
- [Web Services Testing Environment](https://www.afip.gob.ar/ws/documentacion/manual_desarrollador_COMPG_v2_4.pdf)
- [Electronic Invoicing Regulations](https://www.afip.gob.ar/fe/documentos/)

### AFIP Regulations
- [RG 2485/2008 - Electronic Invoicing Base](https://www.afip.gob.ar/fe/documentos/RG2485.pdf)
- [RG 2904/2010 - Electronic Invoicing Implementation](https://www.afip.gob.ar/fe/documentos/RG2904.pdf)
- [RG 3067/2011 - Electronic Invoicing Mandatory](https://www.afip.gob.ar/fe/documentos/RG3067.pdf)
- [RG 3668/2014 - Special Regimes](https://www.afip.gob.ar/fe/documentos/RG3668.pdf)
- [RG 3749/2015 - VAT Conditions](https://www.afip.gob.ar/fe/documentos/RG3749.pdf)
- [RG 4367/2018 - FCE MiPyMEs](https://www.afip.gob.ar/fe/documentos/RG4367.pdf)
- [RG 5616/2024 - Foreign Currency (FEv4)](https://www.afip.gob.ar/fe/documentos/RG5616.pdf)

### Service URLs
- **Testing WSAA**: `https://wsaahomo.afip.gov.ar/ws/services/LoginCms`
- **Production WSAA**: `https://wsaa.afip.gov.ar/ws/services/LoginCms`
- **Testing WSFEv1**: `https://wswhomo.afip.gov.ar/wsfev1/service.asmx`
- **Production WSFEv1**: `https://servicios1.afip.gov.ar/wsfev1/service.asmx`

### Certificate Management
- [Digital Certificate Request](https://www.afip.gob.ar/ws/WSAA/certificados.asp)
- [Certificate Installation Guide](https://www.afip.gob.ar/ws/documentacion/certificados.asp)
- [Testing Environment Certificates](https://www.afip.gob.ar/ws/documentacion/certificados_testing.asp)

## ❓ FAQ

**Q: Do I need to register with AFIP to use this SDK?**
A: Yes, you need to be registered with AFIP and have a valid certificate for electronic invoicing.

**Q: Can I use this in production immediately?**
A: Yes, but make sure to test thoroughly in AFIP's testing environment first.

**Q: What's the difference between Invoice A, B, and C?**
A: 
- Invoice A: For VAT registered entities
- Invoice B: For final consumers (individuals)
- Invoice C: For exempt entities

**Q: How do I handle certificate expiration?**
A: The SDK will throw an `AfipException` when the certificate expires. You'll need to renew it through AFIP.

**Q: Is this SDK thread-safe?**
A: Yes, the SDK is designed to be thread-safe and can be used in multi-threaded applications.

**Q: What's the difference between the core package and the DI package?**
A: The core package (`Afip.Dotnet`) contains the main SDK functionality. The DI package (`Afip.Dotnet.DependencyInjection`) provides dependency injection extensions for easier integration with ASP.NET Core and other DI containers.

## 📞 Support

- **GitHub Issues**: [Report bugs or request features](https://github.com/nruizneiman/Afip.Dotnet/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/nruizneiman/Afip.Dotnet/discussions)
- **Documentation**: [Comprehensive guides and API reference](https://nruizneiman.github.io/Afip.Dotnet/)

---

Made with ❤️ for the Argentine .NET community
