# AFIP .NET SDK - Dependency Injection Example

This example demonstrates how to use the AFIP .NET SDK with Microsoft.Extensions.DependencyInjection in various .NET application types including console applications, ASP.NET Core, and Worker Services.

## Features Demonstrated

- ✅ **Service Registration**: How to register AFIP services with dependency injection
- ✅ **Configuration Management**: Multiple ways to configure AFIP services
- ✅ **Service Interfaces**: Creating service abstractions for better testability
- ✅ **Logging Integration**: Using Microsoft.Extensions.Logging throughout
- ✅ **Error Handling**: Proper exception handling and logging
- ✅ **Async Operations**: Full async/await pattern implementation
- ✅ **Resource Management**: Proper disposal and lifetime management

## Prerequisites

Before running this example, you need:

1. **Valid CUIT**: A valid CUIT number registered with AFIP
2. **Digital Certificate**: A PKCS#12 (.p12/.pfx) certificate file issued by AFIP
3. **Certificate Password**: The password for your certificate file
4. **.NET 6.0 or later**: This example targets .NET 6.0

## Setup Instructions

### 1. Update Configuration

Edit the `Program.cs` file and update the following values with your actual AFIP credentials:

```csharp
services.AddAfipServicesForTesting(
    cuit: 20123456789, // ← Replace with your actual CUIT
    certificatePath: "certificates/testing.p12", // ← Path to your certificate
    certificatePassword: "test-password" // ← Your certificate password
);
```

### 2. Certificate Setup

Create a `certificates` folder in the project directory and place your AFIP certificate file there:

```
examples/DependencyInjection/
├── certificates/
│   └── testing.p12  (or production.p12)
├── Program.cs
├── DependencyInjection.csproj
└── README.md
```

### 3. Environment Selection

The example is configured for the **testing environment** by default. To use production:

```csharp
// Change from:
services.AddAfipServicesForTesting(...)

// To:
services.AddAfipServicesForProduction(...)
```

## Running the Example

### Option 1: From the solution root

```bash
dotnet run --project examples/DependencyInjection
```

### Option 2: From the example directory

```bash
cd examples/DependencyInjection
dotnet run
```

### Option 3: Using Visual Studio

1. Set `DependencyInjection` as the startup project
2. Press F5 or Ctrl+F5 to run

## What the Example Does

The example performs the following operations in sequence:

1. **Service Status Check**: Verifies that AFIP services are available
2. **Parameter Tables**: Retrieves and displays AFIP parameter tables (invoice types, document types, VAT rates)
3. **Next Invoice Number**: Gets the next available invoice number for point of sale 1
4. **Invoice Creation**: Creates and authorizes a sample invoice
5. **Invoice Retrieval**: Retrieves the created invoice to verify it was saved correctly

## Expected Output

```
AFIP .NET SDK - Dependency Injection Example
============================================

=== AFIP Service Status ===
Application: OK
Authentication: OK
Database: OK

=== AFIP Parameter Tables ===
Invoice Types (15 found):
  1: Factura A
  2: Nota de Débito A
  3: Nota de Crédito A
  4: Recibo A
  5: Nota de Venta al Contado A
  ... and 10 more

Document Types (12 found):
  80: CUIT
  86: CUIL
  87: CDI
  89: LE
  90: LC
  ... and 7 more

VAT Rates (9 found):
  1: 0%
  2: 10.5%
  3: 21%
  4: 27%
  5: 5%
  6: 2.5%
  7: Exento
  8: No Gravado
  9: No Alcanzado

=== Invoice Operations ===
Next invoice number: 123

Creating sample invoice...
✅ Invoice created successfully!
   CAE: 71234567890123
   Invoice Number: 123
   Expiration Date: 2024-02-15

Retrieving created invoice...
✅ Invoice retrieved successfully!
   Total Amount: $121.00
   Net Amount: $100.00
   VAT Amount: $21.00

Press any key to exit...
```

## Architecture Overview

The example demonstrates clean architecture principles:

### Service Interfaces

- `IInvoiceService`: Handles invoice-related operations
- `IParameterService`: Manages AFIP parameter table operations

### Service Implementations

- `InvoiceService`: Implements invoice business logic
- `ParameterService`: Implements parameter retrieval logic
- `ExampleOrchestrator`: Coordinates the entire example workflow

### Dependency Injection Registration

```csharp
// AFIP SDK services
services.AddAfipServicesForTesting(cuit, certPath, certPassword);

// Application services
services.AddScoped<IInvoiceService, InvoiceService>();
services.AddScoped<IParameterService, ParameterService>();
services.AddScoped<ExampleOrchestrator>();
```

## Configuration Options

### Basic Registration

```csharp
services.AddAfipServices(new AfipConfiguration
{
    Environment = AfipEnvironment.Testing,
    Cuit = 20123456789,
    CertificatePath = "certificates/testing.p12",
    CertificatePassword = "password",
    EnableLogging = true
});
```

### Action-based Configuration

```csharp
services.AddAfipServices(config =>
{
    config.Environment = AfipEnvironment.Production;
    config.Cuit = long.Parse(Environment.GetEnvironmentVariable("AFIP_CUIT")!);
    config.CertificatePath = Environment.GetEnvironmentVariable("AFIP_CERT_PATH")!;
    config.CertificatePassword = Environment.GetEnvironmentVariable("AFIP_CERT_PASSWORD")!;
    config.TimeoutSeconds = 60;
    config.EnableLogging = true;
});
```

### Environment-specific Registration

```csharp
// Testing environment
services.AddAfipServicesForTesting(cuit, certPath, certPassword);

// Production environment  
services.AddAfipServicesForProduction(cuit, certPath, certPassword);
```

## Error Handling

The example includes comprehensive error handling:

- **Connection Issues**: Network connectivity problems
- **Authentication Errors**: Invalid certificates or CUIT
- **Service Unavailability**: AFIP service outages
- **Validation Errors**: Invalid invoice data
- **Authorization Failures**: Invoice authorization problems

## Extending the Example

You can extend this example by:

1. **Adding More Services**: Create additional service interfaces for other AFIP operations
2. **Database Integration**: Store invoice data in a database
3. **Background Processing**: Implement queued invoice processing
4. **Web API**: Convert to an ASP.NET Core Web API
5. **Authentication**: Add user authentication and authorization
6. **Multi-tenant**: Support multiple companies/CUITs

## Troubleshooting

### Common Issues

1. **Certificate Not Found**
   - Verify the certificate path is correct
   - Ensure the certificate file exists and is readable

2. **Invalid CUIT**
   - Check that the CUIT is exactly 11 digits
   - Verify it's registered with AFIP

3. **Certificate Password Error**
   - Ensure the password is correct
   - Try importing the certificate manually first

4. **Service Unavailable**
   - Check internet connectivity
   - Verify AFIP services are operational
   - Ensure you're using the correct environment URLs

### Logging

The example includes detailed logging. Check the console output for specific error messages and diagnostic information.

## Next Steps

After running this example successfully:

1. Review the [Basic Usage Example](../BasicUsage/) for simpler scenarios
2. Check the [main README](../../README.md) for comprehensive documentation
3. Explore the [Contributing Guidelines](../../CONTRIBUTING.md) to contribute to the project
4. Review the [API Documentation](../../docs/) for detailed API reference