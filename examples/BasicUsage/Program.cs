using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Afip.Dotnet;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Microsoft.Extensions.Logging;
using System.Linq; // Added for .Take() and .Where()

namespace BasicUsage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Configuration - Replace with your actual values
                var client = AfipClient.CreateForTesting(
                    cuit: 20123456789,
                    certificatePath: "certificate.p12", // Path to your AFIP certificate
                    certificatePassword: "your_password"
                );

                // Test connection
                Console.WriteLine("Testing connection to AFIP...");
                bool isConnected = await client.TestConnectionAsync();
                Console.WriteLine($"Connected: {isConnected}");
                
                if (!isConnected)
                {
                    Console.WriteLine("Failed to connect to AFIP. Please check your configuration.");
                    return;
                }

                // Example 1: Get parameter tables
                await GetParameterTablesExample(client);

                // Example 2: Get last invoice number
                await GetLastInvoiceNumberExample(client);

                // Example 3: Create and authorize an invoice
                await CreateInvoiceExample(client);

                // Example 4: Query an existing invoice
                await QueryInvoiceExample(client);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task GetParameterTablesExample(AfipClient client)
        {
            Console.WriteLine("\n=== Parameter Tables Example ===");

            // Get invoice types
            var invoiceTypes = await client.Parameters.GetInvoiceTypesAsync();
            Console.WriteLine($"Available Invoice Types: {invoiceTypes.Count}");
            foreach (var type in invoiceTypes.Take(5)) // Show first 5
            {
                Console.WriteLine($"  {type.Id}: {type.Description}");
            }

            // Get VAT rates
            var vatRates = await client.Parameters.GetVatRatesAsync();
            Console.WriteLine($"\nAvailable VAT Rates: {vatRates.Count}");
            foreach (var rate in vatRates.Where(r => r.IsActive))
            {
                Console.WriteLine($"  {rate.Id}: {rate.Description}");
            }

            // Get currencies
            var currencies = await client.Parameters.GetCurrenciesAsync();
            Console.WriteLine($"\nAvailable Currencies: {currencies.Count}");
            foreach (var currency in currencies.Take(5))
            {
                Console.WriteLine($"  {currency.Id}: {currency.Description}");
            }

            // Get USD exchange rate
            try
            {
                var usdRate = await client.Parameters.GetCurrencyRateAsync("DOL", DateTime.Today);
                Console.WriteLine($"\nUSD Exchange Rate: {usdRate:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get USD rate: {ex.Message}");
            }
        }

        static async Task GetLastInvoiceNumberExample(AfipClient client)
        {
            Console.WriteLine("\n=== Last Invoice Number Example ===");

            try
            {
                // Get last invoice number for point of sale 1, invoice type C (11)
                var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(1, 11);
                var nextNumber = lastNumber + 1;
                
                Console.WriteLine($"Last Invoice Number (POS 1, Type C): {lastNumber}");
                Console.WriteLine($"Next Invoice Number would be: {nextNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting last invoice number: {ex.Message}");
            }
        }

        static async Task CreateInvoiceExample(AfipClient client)
        {
            Console.WriteLine("\n=== Create Invoice Example ===");

            try
            {
                // Get next invoice number
                var pointOfSale = 1;
                var invoiceType = 11; // Invoice C
                var nextInvoiceNumber = await client.GetNextInvoiceNumberAsync(pointOfSale, invoiceType);

                // Create invoice request
                var request = new InvoiceRequest
                {
                    PointOfSale = pointOfSale,
                    InvoiceType = invoiceType,
                    Concept = 1, // Products
                    DocumentType = 96, // DNI
                    DocumentNumber = 12345678,
                    InvoiceNumberFrom = nextInvoiceNumber,
                    InvoiceNumberTo = nextInvoiceNumber,
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
                        new VatDetail
                        {
                            VatId = 5, // 21%
                            BaseAmount = 100.00m,
                            Amount = 21.00m
                        }
                    }
                };

                Console.WriteLine($"Creating invoice {nextInvoiceNumber} for ${request.TotalAmount}...");

                // Authorize invoice
                var response = await client.ElectronicInvoicing.AuthorizeInvoiceAsync(request);

                Console.WriteLine($"Invoice Authorization Result: {response.Result}");
                Console.WriteLine($"CAE (Authorization Code): {response.AuthorizationCode}");
                Console.WriteLine($"CAE Expiration: {response.AuthorizationExpirationDate:yyyy-MM-dd}");

                // Check for errors
                if (response.Errors?.Any() == true)
                {
                    Console.WriteLine("Errors:");
                    foreach (var error in response.Errors)
                    {
                        Console.WriteLine($"  {error.Code}: {error.Message}");
                    }
                }

                // Check for observations
                if (response.Observations?.Any() == true)
                {
                    Console.WriteLine("Observations:");
                    foreach (var obs in response.Observations)
                    {
                        Console.WriteLine($"  {obs.Code}: {obs.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating invoice: {ex.Message}");
            }
        }

        static async Task QueryInvoiceExample(AfipClient client)
        {
            Console.WriteLine("\n=== Query Invoice Example ===");

            try
            {
                // Query the last authorized invoice
                var lastNumber = await client.ElectronicInvoicing.GetLastInvoiceNumberAsync(1, 11);
                
                if (lastNumber > 0)
                {
                    var invoice = await client.ElectronicInvoicing.QueryInvoiceAsync(1, 11, lastNumber);
                    
                    Console.WriteLine($"Queried Invoice {lastNumber}:");
                    Console.WriteLine($"  Authorization Code: {invoice.AuthorizationCode}");
                    Console.WriteLine($"  Result: {invoice.Result}");
                    Console.WriteLine($"  Processed Date: {invoice.ProcessedDate:yyyy-MM-dd HH:mm}");
                }
                else
                {
                    Console.WriteLine("No invoices found to query.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying invoice: {ex.Message}");
            }
        }

        static async Task BatchInvoiceExample(AfipClient client)
        {
            Console.WriteLine("\n=== Batch Invoice Example ===");

            try
            {
                var requests = new List<InvoiceRequest>();
                var pointOfSale = 1;
                var invoiceType = 11;
                var baseInvoiceNumber = await client.GetNextInvoiceNumberAsync(pointOfSale, invoiceType);

                // Create 3 invoices
                for (int i = 0; i < 3; i++)
                {
                    var request = new InvoiceRequest
                    {
                        PointOfSale = pointOfSale,
                        InvoiceType = invoiceType,
                        Concept = 1,
                        DocumentType = 96,
                        DocumentNumber = 12345678 + i,
                        InvoiceNumberFrom = baseInvoiceNumber + i,
                        InvoiceNumberTo = baseInvoiceNumber + i,
                        InvoiceDate = DateTime.Today,
                        TotalAmount = 100 + (i * 50),
                        NetAmount = (100 + (i * 50)) / 1.21m,
                        VatAmount = (100 + (i * 50)) - ((100 + (i * 50)) / 1.21m),
                        CurrencyId = "PES",
                        CurrencyRate = 1.0m,
                        VatDetails = new List<VatDetail>
                        {
                            new VatDetail
                            {
                                VatId = 5, // 21%
                                BaseAmount = (100 + (i * 50)) / 1.21m,
                                Amount = (100 + (i * 50)) - ((100 + (i * 50)) / 1.21m)
                            }
                        }
                    };
                    requests.Add(request);
                }

                Console.WriteLine($"Creating batch of {requests.Count} invoices...");

                var responses = await client.ElectronicInvoicing.AuthorizeInvoicesAsync(requests);

                foreach (var response in responses)
                {
                    Console.WriteLine($"Invoice {response.InvoiceNumber}: {response.Result} - CAE: {response.AuthorizationCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in batch invoice creation: {ex.Message}");
            }
        }
    }
}