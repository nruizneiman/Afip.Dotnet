# AFIP .NET SDK

[![NuGet](https://img.shields.io/nuget/v/Afip.Dotnet.svg)](https://www.nuget.org/packages/Afip.Dotnet/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive .NET Standard SDK for integrating with AFIP (ARCA) web services, including electronic invoicing, authentication, and parameter queries.

## Features

- **Electronic Invoicing (WSFEv1)** - Create, authorize, and query electronic invoices
- **Authentication (WSAA)** - Certificate-based authentication with automatic ticket caching
- **Parameter Tables** - Query AFIP parameter tables (invoice types, currencies, VAT rates, etc.)
- **Multiple Environments** - Support for both testing and production environments
- **Async/Await** - Full asynchronous API with cancellation token support
- **Logging Support** - Integrated with Microsoft.Extensions.Logging
- **Robust Error Handling** - Comprehensive exception handling and validation
- **Latest Regulations** - Supports RG5616/2024 (FEv4) and other recent AFIP regulations

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Afip.Dotnet
```

Or via Package Manager Console:

```powershell
Install-Package Afip.Dotnet
```

## Quick Start

### 1. Certificate Setup

First, you need to obtain a digital certificate from AFIP and convert it to PKCS#12 format:

```bash
# Generate private key
openssl genrsa -out private_key.key 2048

# Generate certificate request
openssl req -new -key private_key.key -out certificate_request.csr

# Submit CSR to AFIP and download the certificate
# Convert to PKCS#12 format
openssl pkcs12 -export -out certificate.p12 -inkey private_key.key -in certificate.crt
```

### 2. Basic Usage

```csharp
using Afip.Dotnet;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;

// Create client for testing environment
var client = AfipClient.CreateForTesting(
    cuit: 20123456789, 
    certificatePath: "path/to/certificate.p12", 
    certificatePassword: "certificate_password"
);

// Test connection
bool isConnected = await client.TestConnectionAsync();
Console.WriteLine($"Connected to AFIP: {isConnected}");

// Create and authorize an invoice
var invoiceRequest = new InvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 11, // Invoice C
    Concept = 1, // Products
    DocumentType = 96, // DNI
    DocumentNumber = 12345678,
    InvoiceNumberFrom = await client.GetNextInvoiceNumberAsync(1, 11),
    InvoiceNumberTo = await client.GetNextInvoiceNumberAsync(1, 11),
    InvoiceDate = DateTime.Today,
    TotalAmount = 121.00m,
    NetAmount = 100.00m,
    VatAmount = 21.00m,
    CurrencyId = "PES",
    CurrencyRate = 1.0m,
    VatDetails = new List<VatDetail>
    {
        new VatDetail
        {
            VatRate = 21.0m,
            BaseAmount = 100.00m,
            VatAmount = 21.00m
        }
    }
};

var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(invoiceRequest);
Console.WriteLine($"Invoice authorized with CAE: {response.AuthorizationCode}");
```

## Configuration

### Basic Configuration

```csharp
var configuration = new AfipConfiguration
{
    Environment = AfipEnvironment.Testing, // or AfipEnvironment.Production
    Cuit = 20123456789,
    CertificatePath = "path/to/certificate.p12",
    CertificatePassword = "password",
    TimeoutSeconds = 30,
    EnableLogging = true
};

var client = new AfipClient(configuration);
```

### Advanced Configuration

```csharp
var configuration = new AfipConfiguration
{
    Environment = AfipEnvironment.Testing,
    Cuit = 20123456789,
    CertificatePath = "certificate.p12",
    CertificatePassword = "password",
    TimeoutSeconds = 60,
    EnableLogging = true,
    // Custom URLs for testing
    CustomWsaaUrl = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
    CustomWsfev1Url = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx"
};
```

## Electronic Invoicing

### Single Invoice Authorization

```csharp
var request = new InvoiceRequest
{
    PointOfSale = 1,
    InvoiceType = 1, // Invoice A
    Concept = 1, // Products
    DocumentType = 80, // CUIT
    DocumentNumber = 20987654321,
    InvoiceNumberFrom = 1,
    InvoiceNumberTo = 1,
    InvoiceDate = DateTime.Today,
    TotalAmount = 121.00m,
    NonTaxableAmount = 0m,
    NetAmount = 100.00m,
    ExemptAmount = 0m,
    VatAmount = 21.00m,
    TaxAmount = 0m,
    CurrencyId = "PES",
    CurrencyRate = 1.0m,
    VatDetails = new List<VatDetail>
    {
        new VatDetail { VatRate = 21.0m, BaseAmount = 100.00m, VatAmount = 21.00m }
    }
};

var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
```

### Batch Invoice Authorization

```csharp
var requests = new List<InvoiceRequest> { request1, request2, request3 };
var responses = await client.ElectronicInvoicing.AuthorizeInvoicesAsync(requests);

foreach (var response in responses)
{
    Console.WriteLine($"Invoice {response.InvoiceNumber}: {response.Result}");
}
```

### Query Invoice Information

```csharp
var invoice = await client.ElectronicInvoicing.QueryInvoiceAsync(
    pointOfSale: 1, 
    invoiceType: 1, 
    invoiceNumber: 123
);
```

### Get Last Invoice Number

```csharp
var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(
    pointOfSale: 1, 
    invoiceType: 1
);
var nextNumber = lastNumber + 1;
```

## Parameter Tables

### Get Invoice Types

```csharp
var invoiceTypes = await client.Parameters.GetInvoiceTypesAsync();
foreach (var type in invoiceTypes)
{
    Console.WriteLine($"{type.Id}: {type.Description}");
}
```

### Get VAT Rates

```csharp
var vatRates = await client.Parameters.GetVatRatesAsync();
var activeRates = vatRates.Where(r => r.IsActive).ToList();
```

### Get Currencies and Exchange Rates

```csharp
var currencies = await client.Parameters.GetCurrenciesAsync();
var usdRate = await client.Parameters.GetCurrencyRateAsync("DOL", DateTime.Today);
```

### Other Parameter Tables

```csharp
// Document types (DNI, CUIT, etc.)
var documentTypes = await client.Parameters.GetDocumentTypesAsync();

// Concept types (Products, Services, etc.)
var conceptTypes = await client.Parameters.GetConceptTypesAsync();

// Tax types
var taxTypes = await client.Parameters.GetTaxTypesAsync();

// VAT conditions for receivers
var vatConditions = await client.Parameters.GetReceiverVatConditionsAsync("A");
```

## Invoice Types and Regulations

### Supported Invoice Types

| Code | Description | Regulation |
|------|-------------|------------|
| 1 | Invoice A | General |
| 6 | Invoice B | General |
| 11 | Invoice C | General |
| 201 | Credit Note A | General |
| 206 | Credit Note B | General |
| 211 | Credit Note C | General |
| 202 | Debit Note A | General |
| 207 | Debit Note B | General |
| 212 | Debit Note C | General |
| 19 | Electronic Invoice FCE A (MiPyMEs) | RG4367/2018 |
| 20 | Electronic Invoice FCE B (MiPyMEs) | RG4367/2018 |
| 21 | Electronic Invoice FCE C (MiPyMEs) | RG4367/2018 |

### Foreign Currency Support (FEv4)

The SDK supports the latest RG5616/2024 regulation for foreign currency transactions:

```csharp
var request = new InvoiceRequest
{
    // ... basic fields ...
    CurrencyId = "DOL", // US Dollar
    CurrencyRate = 350.50m, // Exchange rate
    PayInSameForeignCurrency = true, // FEv4 requirement
    ReceiverVatCondition = 1, // Required for FEv4
    // ... other fields ...
};
```

## Error Handling

```csharp
try
{
    var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);
    
    if (response.Errors?.Any() == true)
    {
        foreach (var error in response.Errors)
        {
            Console.WriteLine($"Error {error.Code}: {error.Message}");
        }
    }
    
    if (response.Observations?.Any() == true)
    {
        foreach (var obs in response.Observations)
        {
            Console.WriteLine($"Observation {obs.Code}: {obs.Message}");
        }
    }
}
catch (AfipException ex)
{
    Console.WriteLine($"AFIP Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
}
```

## Logging

The SDK integrates with Microsoft.Extensions.Logging:

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<AfipClient>();
var client = new AfipClient(configuration, logger);
```

## Testing

For testing, use the AFIP testing environment with your testing certificate:

```csharp
var client = AfipClient.CreateForTesting(
    cuit: 20123456789,
    certificatePath: "testing_certificate.p12",
    certificatePassword: "password"
);

// Test endpoints:
// WSAA: https://wsaahomo.afip.gov.ar/ws/services/LoginCms
// WSFEv1: https://wswhomo.afip.gov.ar/wsfev1/service.asmx
```

## Production Deployment

For production, ensure you:

1. Use production certificates from AFIP
2. Set environment to `AfipEnvironment.Production`
3. Enable logging for monitoring
4. Implement proper error handling
5. Cache authentication tickets appropriately

```csharp
var client = AfipClient.CreateForProduction(
    cuit: yourProductionCuit,
    certificatePath: "production_certificate.p12",
    certificatePassword: Environment.GetEnvironmentVariable("AFIP_CERT_PASSWORD")
);
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This SDK is not officially endorsed by AFIP. Use at your own risk and ensure compliance with current AFIP regulations.

## Support

- Check the [AFIP official documentation](https://www.afip.gob.ar/fe/documentos/)
- Review the [RG5616/2024 regulation](https://www.afip.gob.ar/fe/documentos/RG5616.pdf) for FEv4 requirements
- For SDK issues, please open a GitHub issue

## Related Links

- [AFIP Official Website](https://www.afip.gob.ar/)
- [Electronic Invoicing Documentation](https://www.afip.gob.ar/fe/)
- [Testing Environment Access](https://www.afip.gob.ar/fe/documentos/manual_desarrollador_COMPG_v2_4.pdf)
