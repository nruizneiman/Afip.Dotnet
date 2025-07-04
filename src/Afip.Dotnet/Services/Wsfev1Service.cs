using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Afip.Dotnet.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Implementation of WSFEv1 (Electronic Invoicing Version 1) service
    /// </summary>
    public class Wsfev1Service : IWsfev1Service
    {
        private readonly AfipConfiguration _configuration;
        private readonly IWsaaService _wsaaService;
        private readonly ILogger<Wsfev1Service>? _logger;

        public Wsfev1Service(AfipConfiguration configuration, IWsaaService wsaaService, ILogger<Wsfev1Service>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _wsaaService = wsaaService ?? throw new ArgumentNullException(nameof(wsaaService));
            _logger = logger;
        }

        public async Task<InvoiceResponse> AuthorizeInvoiceAsync(InvoiceRequest request, CancellationToken cancellationToken = default)
        {
            var responses = await AuthorizeInvoicesAsync(new List<InvoiceRequest> { request }, cancellationToken);
            return responses.First();
        }

        public async Task<List<InvoiceResponse>> AuthorizeInvoicesAsync(List<InvoiceRequest> requests, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Authorizing {Count} invoices", requests.Count);

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var responses = new List<InvoiceResponse>();

                foreach (var request in requests)
                {
                    var authInfo = new FEAuthRequest
                    {
                        Token = ticket.Token,
                        Sign = ticket.Sign,
                        Cuit = _configuration.Cuit
                    };

                    var invoiceRequest = MapToFERequest(request);
                    var response = await Task.Run(() => channel.FECAESolicitar(authInfo, invoiceRequest), cancellationToken);

                    var invoiceResponse = MapFromFEResponse(response, request);
                    responses.Add(invoiceResponse);

                    _logger?.LogInformation("Invoice authorized: Point of Sale {PointOfSale}, Number {InvoiceNumber}, CAE {CAE}",
                        request.PointOfSale, request.InvoiceNumberFrom, invoiceResponse.Cae);
                }

                return responses;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to authorize invoices");
                throw new AfipException("Failed to authorize invoices", ex);
            }
        }

        public async Task<long> GetLastInvoiceNumberAsync(int pointOfSale, int invoiceType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting last invoice number for POS {PointOfSale}, Type {InvoiceType}", pointOfSale, invoiceType);

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FECompUltimoAutorizado(authInfo, pointOfSale, invoiceType), cancellationToken);

                if (response?.CbteNro == null)
                    return 0;

                return response.CbteNro.Value;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get last invoice number");
                throw new AfipException("Failed to get last invoice number", ex);
            }
        }

        public async Task<InvoiceResponse> QueryInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Querying invoice: POS {PointOfSale}, Type {InvoiceType}, Number {InvoiceNumber}",
                    pointOfSale, invoiceType, invoiceNumber);

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FECompConsultar(authInfo, new FECompConsRequest
                {
                    PtoVta = pointOfSale,
                    CbteTipo = invoiceType,
                    CbteNro = invoiceNumber
                }), cancellationToken);

                return MapFromQueryResponse(response);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to query invoice");
                throw new AfipException("Failed to query invoice", ex);
            }
        }

        public async Task<ServiceStatus> CheckServiceStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEDummy(), cancellationToken);

                return new ServiceStatus
                {
                    AppServer = response?.AppServer ?? "Unknown",
                    DbServer = response?.DbServer ?? "Unknown", 
                    AuthServer = response?.AuthServer ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to check service status");
                return new ServiceStatus
                {
                    AppServer = "Error",
                    DbServer = "Error",
                    AuthServer = "Error"
                };
            }
        }

        private IWsfev1ServiceChannel CreateServiceChannel()
        {
            var binding = new BasicHttpBinding
            {
                Security = { Mode = BasicHttpSecurityMode.Transport },
                MaxReceivedMessageSize = 1024 * 1024 // 1MB
            };

            var endpoint = new EndpointAddress(_configuration.GetWsfev1Url());
            var factory = new ChannelFactory<IWsfev1ServiceChannel>(binding, endpoint);

            return factory.CreateChannel();
        }

        private FECAERequest MapToFERequest(InvoiceRequest request)
        {
            return new FECAERequest
            {
                FeCabReq = new FECAECabRequest
                {
                    CantReg = 1,
                    PtoVta = request.PointOfSale,
                    CbteTipo = request.InvoiceType
                },
                FeDetReq = new[]
                {
                    new FECAEDetRequest
                    {
                        Concepto = request.Concept,
                        DocTipo = request.DocumentType,
                        DocNro = request.DocumentNumber,
                        CbteDesde = request.InvoiceNumberFrom,
                        CbteHasta = request.InvoiceNumberTo,
                        CbteFch = request.InvoiceDate.ToString("yyyyMMdd"),
                        ImpTotal = request.TotalAmount,
                        ImpTotConc = request.NonTaxableAmount,
                        ImpNeto = request.NetAmount,
                        ImpOpEx = request.ExemptAmount,
                        ImpTrib = request.TaxAmount,
                        ImpIVA = request.VatAmount,
                        MonId = request.CurrencyId,
                        MonCotiz = request.CurrencyRate,
                        FchServDesde = request.ServiceDateFrom?.ToString("yyyyMMdd"),
                        FchServHasta = request.ServiceDateTo?.ToString("yyyyMMdd"),
                        FchVtoPago = request.PaymentDueDate?.ToString("yyyyMMdd"),
                        Iva = request.VatDetails?.Select(v => new AlicIva
                        {
                            Id = GetVatId(v.VatRateId),
                            BaseImp = v.BaseAmount,
                            Importe = v.VatAmount
                        }).ToArray(),
                        Tributos = request.TaxDetails?.Select(t => new Tributo
                        {
                            Id = t.TaxTypeId,
                            Desc = t.Description,
                            BaseImp = t.BaseAmount,
                            Alic = t.TaxRate,
                            Importe = t.TaxAmount
                        }).ToArray(),
                        CbtesAsoc = request.AssociatedInvoices?.Select(a => new CbteAsoc
                        {
                            Tipo = a.InvoiceType,
                            PtoVta = a.PointOfSale,
                            Nro = a.InvoiceNumber
                        }).ToArray(),
                        Opcionales = request.OptionalData?.Select(o => new Opcional
                        {
                            Id = o.Id.ToString(),
                            Valor = o.Value
                        }).ToArray()
                    }
                }
            };
        }

        private InvoiceResponse MapFromFEResponse(FECAEResponse response, InvoiceRequest originalRequest)
        {
            var detail = response?.FeDetResp?.FirstOrDefault();
            
            return new InvoiceResponse
            {
                PointOfSale = originalRequest.PointOfSale,
                InvoiceType = originalRequest.InvoiceType,
                InvoiceNumber = originalRequest.InvoiceNumberFrom,
                AuthorizationCode = detail?.CAE ?? string.Empty,
                Cae = detail?.CAE ?? string.Empty,
                AuthorizationExpirationDate = DateTime.TryParseExact(detail?.CAEFchVto, "yyyyMMdd", null, 
                    System.Globalization.DateTimeStyles.None, out var expDate) ? expDate : (DateTime?)null,
                CaeExpirationDate = DateTime.TryParseExact(detail?.CAEFchVto, "yyyyMMdd", null, 
                    System.Globalization.DateTimeStyles.None, out var expDate2) ? expDate2 : (DateTime?)null,
                Result = detail?.Resultado ?? string.Empty,
                ProcessedDate = DateTime.UtcNow,
                ProcessingDate = DateTime.UtcNow,
                Observations = detail?.Observaciones?.Select(o => new AfipObservation
                {
                    Code = o.Code,
                    Message = o.Msg
                }).ToList() ?? new List<AfipObservation>(),
                Errors = response?.Errors?.Select(e => new InvoiceError
                {
                    Code = e.Code,
                    Message = e.Msg
                }).ToList() ?? new List<InvoiceError>()
            };
        }

        private InvoiceResponse MapFromQueryResponse(FECompConsResponse response)
        {
            // Implementation would depend on the actual AFIP service response structure
            // This is a simplified version
            return new InvoiceResponse
            {
                // Map fields from response
            };
        }

        private int GetVatId(decimal vatRate)
        {
            return vatRate switch
            {
                0m => 3,      // 0%
                2.5m => 9,    // 2.5%
                5m => 8,      // 5%
                10.5m => 4,   // 10.5%
                21m => 5,     // 21%
                27m => 6,     // 27%
                _ => 3        // Default to 0%
            };
        }
    }

    // SOAP service contract interfaces and data contracts would go here
    // These would typically be auto-generated from WSDL but simplified for this example
    
    [ServiceContract]
    internal interface IWsfev1ServiceChannel
    {
        [OperationContract]
        FECAEResponse FECAESolicitar(FEAuthRequest auth, FECAERequest request);
        
        [OperationContract]
        FERecuperaLastCbteResponse FECompUltimoAutorizado(FEAuthRequest auth, int ptoVta, int cbteTipo);
        
        [OperationContract]
        FECompConsResponse FECompConsultar(FEAuthRequest auth, FECompConsRequest request);
        
        [OperationContract]
        FEDummyResponse FEDummy();
    }

    // Simplified data contracts - in a real implementation these would be generated from WSDL
    public class FEAuthRequest
    {
        public string Token { get; set; }
        public string Sign { get; set; }
        public long Cuit { get; set; }
    }

    public class FECAERequest
    {
        public FECAECabRequest FeCabReq { get; set; }
        public FECAEDetRequest[] FeDetReq { get; set; }
    }

    public class FECAECabRequest
    {
        public int CantReg { get; set; }
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
    }

    public class FECAEDetRequest
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public long CbteDesde { get; set; }
        public long CbteHasta { get; set; }
        public string CbteFch { get; set; }
        public decimal ImpTotal { get; set; }
        public decimal ImpTotConc { get; set; }
        public decimal ImpNeto { get; set; }
        public decimal ImpOpEx { get; set; }
        public decimal ImpTrib { get; set; }
        public decimal ImpIVA { get; set; }
        public string MonId { get; set; }
        public decimal MonCotiz { get; set; }
        public string FchServDesde { get; set; }
        public string FchServHasta { get; set; }
        public string FchVtoPago { get; set; }
        public AlicIva[] Iva { get; set; }
        public Tributo[] Tributos { get; set; }
        public CbteAsoc[] CbtesAsoc { get; set; }
        public Opcional[] Opcionales { get; set; }
    }

    public class FECAEResponse
    {
        public FECAEDetResponse[] FeDetResp { get; set; }
        public Err[] Errors { get; set; }
    }

    public class FECAEDetResponse
    {
        public string CAE { get; set; }
        public string CAEFchVto { get; set; }
        public string Resultado { get; set; }
        public Obs[] Observaciones { get; set; }
    }

    public class AlicIva
    {
        public int Id { get; set; }
        public decimal BaseImp { get; set; }
        public decimal Importe { get; set; }
    }

    public class Tributo
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public decimal BaseImp { get; set; }
        public decimal Alic { get; set; }
        public decimal Importe { get; set; }
    }

    public class CbteAsoc
    {
        public int Tipo { get; set; }
        public int PtoVta { get; set; }
        public long Nro { get; set; }
    }

    public class Opcional
    {
        public string Id { get; set; }
        public string Valor { get; set; }
    }

    public class Obs
    {
        public int Code { get; set; }
        public string Msg { get; set; }
    }

    public class Err
    {
        public int Code { get; set; }
        public string Msg { get; set; }
    }

    public class FERecuperaLastCbteResponse
    {
        public long? CbteNro { get; set; }
    }

    public class FECompConsRequest
    {
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
        public long CbteNro { get; set; }
    }

    public class FECompConsResponse
    {
        // Simplified - would contain invoice details
    }

    public class FEDummyResponse
    {
        public string AppServer { get; set; }
        public string DbServer { get; set; }
        public string AuthServer { get; set; }
    }
}