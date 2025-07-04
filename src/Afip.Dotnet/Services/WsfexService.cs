using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Models.Invoice;
using Afip.Dotnet.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Implementation of WSFEX service for export invoicing
    /// </summary>
    public class WsfexService : IWsfexService
    {
        private readonly IWsaaService _wsaaService;
        private readonly IAfipParametersService _parametersService;
        private readonly ILogger<WsfexService> _logger;
        private readonly AfipConfiguration _configuration;

        public WsfexService(
            IWsaaService wsaaService,
            IAfipParametersService parametersService,
            ILogger<WsfexService> logger,
            AfipConfiguration configuration)
        {
            _wsaaService = wsaaService ?? throw new ArgumentNullException(nameof(wsaaService));
            _parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc/>
        public async Task<ServiceStatus> CheckServiceStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Checking WSFEX service status");

                var channel = CreateServiceChannel();
                var response = await Task.Run(() => channel.FEXDummy(), cancellationToken);

                return new ServiceStatus
                {
                    AppServer = response.AppServer,
                    DbServer = response.DbServer,
                    AuthServer = response.AuthServer,
                    StatusMessage = "WSFEX service is available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking WSFEX service status");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetLastInvoiceNumberAsync(int pointOfSale, int invoiceType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting last invoice number for point of sale {PointOfSale} and invoice type {InvoiceType}", pointOfSale, invoiceType);

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetLast_CMP(authInfo, pointOfSale, invoiceType), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX error: {error.Msg} (Code: {error.Code})");
                }

                return response.CbteNro;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last invoice number for point of sale {PointOfSale} and invoice type {InvoiceType}", pointOfSale, invoiceType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ExportInvoiceResponse> AuthorizeExportInvoiceAsync(ExportInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Authorizing export invoice for point of sale {PointOfSale} and invoice type {InvoiceType}", request.PointOfSale, request.InvoiceType);

                ValidateExportInvoiceRequest(request);

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var fexRequest = MapToFexRequest(request);
                var response = await Task.Run(() => channel.FEXAuthorize(authInfo, fexRequest), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX authorization error: {error.Msg} (Code: {error.Code})");
                }

                return MapToExportInvoiceResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing export invoice for point of sale {PointOfSale} and invoice type {InvoiceType}", request.PointOfSale, request.InvoiceType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ExportInvoiceResponse?> QueryExportInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Querying export invoice for point of sale {PointOfSale}, invoice type {InvoiceType}, and invoice number {InvoiceNumber}", pointOfSale, invoiceType, invoiceNumber);

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGet_CMP(authInfo, pointOfSale, invoiceType, invoiceNumber), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    if (error.Code == 15002) // Invoice not found
                        return null;
                    
                    throw new AfipException($"WSFEX query error: {error.Msg} (Code: {error.Code})");
                }

                return MapToExportInvoiceResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying export invoice for point of sale {PointOfSale}, invoice type {InvoiceType}, and invoice number {InvoiceNumber}", pointOfSale, invoiceType, invoiceNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportInvoiceTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export invoice types");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Cbte(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export invoice types");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportDocumentTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export document types");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Doc(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export document types");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export currencies");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Mon(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export currencies");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportDestinationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export destinations");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Dst_pais(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export destinations");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportIncotermsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export incoterms");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Incoterms(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export incoterms");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportLanguagesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export languages");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Idiomas(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export languages");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetExportUnitsOfMeasurementAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting export units of measurement");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsfex", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEXAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_UMed(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSFEX parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export units of measurement");
                throw;
            }
        }

        private void ValidateExportInvoiceRequest(ExportInvoiceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.PointOfSale <= 0)
                throw new ArgumentException("Point of sale must be greater than 0", nameof(request.PointOfSale));

            if (request.InvoiceType <= 0)
                throw new ArgumentException("Invoice type must be greater than 0", nameof(request.InvoiceType));

            if (request.InvoiceNumberFrom <= 0)
                throw new ArgumentException("Invoice number from must be greater than 0", nameof(request.InvoiceNumberFrom));

            if (request.InvoiceNumberTo <= 0)
                throw new ArgumentException("Invoice number to must be greater than 0", nameof(request.InvoiceNumberTo));

            if (request.InvoiceNumberFrom > request.InvoiceNumberTo)
                throw new ArgumentException("Invoice number from cannot be greater than invoice number to");

            if (string.IsNullOrWhiteSpace(request.ReceiverName))
                throw new ArgumentException("Receiver name is required", nameof(request.ReceiverName));

            if (string.IsNullOrWhiteSpace(request.CurrencyId))
                throw new ArgumentException("Currency ID is required", nameof(request.CurrencyId));

            if (request.TotalAmount <= 0)
                throw new ArgumentException("Total amount must be greater than 0", nameof(request.TotalAmount));
        }

        private IWsfexServiceChannel CreateServiceChannel()
        {
            var binding = new BasicHttpBinding
            {
                Security = { Mode = BasicHttpSecurityMode.Transport },
                MaxReceivedMessageSize = 1024 * 1024 // 1MB
            };

            var endpoint = new EndpointAddress(_configuration.GetWsfexUrl());
            var factory = new ChannelFactory<IWsfexServiceChannel>(binding, endpoint);

            return factory.CreateChannel();
        }

        private FEXRequest MapToFexRequest(ExportInvoiceRequest request)
        {
            return new FEXRequest
            {
                FeCabReq = new FEXCabRequest
                {
                    CantReg = 1,
                    PtoVta = request.PointOfSale,
                    CbteTipo = request.InvoiceType
                },
                FeDetReq = new FEXDetRequest[]
                {
                    new FEXDetRequest
                    {
                        Concepto = 1, // Products
                        DocTipo = request.DocumentType,
                        DocNro = request.DocumentNumber,
                        CbteDesde = request.InvoiceNumberFrom,
                        CbteHasta = request.InvoiceNumberTo,
                        CbteFch = request.InvoiceDate.ToString("yyyyMMdd"),
                        ImpTotal = request.TotalAmount,
                        ImpTotConc = request.NetAmount,
                        ImpNeto = request.NetAmount,
                        ImpOpEx = 0,
                        ImpTrib = request.TaxDetails?.Sum(t => t.Amount) ?? 0,
                        ImpIVA = request.VatAmount,
                        MonId = request.CurrencyId,
                        MonCotiz = request.CurrencyRate,
                        FchServDesde = request.InvoiceDate.ToString("yyyyMMdd"),
                        FchServHasta = request.InvoiceDate.ToString("yyyyMMdd"),
                        FchVtoPago = request.InvoiceDate.ToString("yyyyMMdd"),
                        Iva = request.VatDetails?.Select(v => new FEXAlicIva
                        {
                            Id = v.VatId,
                            BaseImp = v.BaseAmount,
                            Importe = v.Amount
                        }).ToArray() ?? new FEXAlicIva[0],
                        Tributos = request.TaxDetails?.Select(t => new FEXTributo
                        {
                            Id = t.TaxId,
                            Desc = t.Description,
                            BaseImp = t.BaseAmount,
                            Alic = t.Rate,
                            Importe = t.Amount
                        }).ToArray() ?? new FEXTributo[0],
                        CbtesAsoc = request.AssociatedInvoices?.Select(a => new FEXCbteAsoc
                        {
                            Tipo = a.InvoiceType,
                            PtoVta = a.PointOfSale,
                            Nro = a.InvoiceNumber
                        }).ToArray() ?? new FEXCbteAsoc[0],
                        Opcionales = request.OptionalData?.Select(o => new FEXOpcional
                        {
                            Id = o.Id,
                            Valor = o.Value
                        }).ToArray() ?? new FEXOpcional[0]
                    }
                }
            };
        }

        private ExportInvoiceResponse MapToExportInvoiceResponse(FEXResponse response)
        {
            if (response.FeDetResp == null || response.FeDetResp.Length == 0)
                throw new AfipException("Invalid WSFEX response: no invoice details");

            var detResponse = response.FeDetResp[0];
            
            return new ExportInvoiceResponse
            {
                PointOfSale = response.FeCabResp?.PtoVta ?? 0,
                InvoiceType = response.FeCabResp?.CbteTipo ?? 0,
                InvoiceNumber = detResponse.CbteDesde,
                InvoiceDate = DateTime.ParseExact(detResponse.CbteFch, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None),
                CAE = detResponse.CAE,
                CAEExpirationDate = DateTime.ParseExact(detResponse.CAEFchVto, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None),
                Result = detResponse.Resultado,
                Observations = detResponse.Observaciones?.Select(o => new Observation
                {
                    Code = o.Code,
                    Message = o.Msg
                }).ToList() ?? new List<Observation>()
            };
        }

        private List<ParameterItem> MapToParameterItems(FEXParamItem[] items)
        {
            var result = new List<ParameterItem>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    result.Add(new ParameterItem
                    {
                        Id = item.Id.ToString(),
                        Description = item.Desc,
                        ValidFrom = DateTime.TryParseExact(item.FchDesde, "yyyyMMdd", null,
                            System.Globalization.DateTimeStyles.None, out var fromDate) ? fromDate : (DateTime?)null,
                        ValidTo = DateTime.TryParseExact(item.FchHasta, "yyyyMMdd", null,
                            System.Globalization.DateTimeStyles.None, out var toDate) ? toDate : (DateTime?)null
                    });
                }
            }
            return result;
        }
    }

    // WSFEX SOAP service contracts and data contracts
    [ServiceContract]
    internal interface IWsfexServiceChannel
    {
        [OperationContract]
        FEXDummyResponse FEXDummy();
        
        [OperationContract]
        FEXLastCMPResponse FEXGetLast_CMP(FEXAuthRequest auth, int ptoVta, int cbteTipo);
        
        [OperationContract]
        FEXResponse FEXAuthorize(FEXAuthRequest auth, FEXRequest request);
        
        [OperationContract]
        FEXResponse FEXGet_CMP(FEXAuthRequest auth, int ptoVta, int cbteTipo, long cbteNro);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_Tipo_Cbte(FEXAuthRequest auth);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_Tipo_Doc(FEXAuthRequest auth);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_Mon(FEXAuthRequest auth);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_Dst_pais(FEXAuthRequest auth);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_Incoterms(FEXAuthRequest auth);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_Idiomas(FEXAuthRequest auth);
        
        [OperationContract]
        FEXParamResponse FEXGetPARAM_UMed(FEXAuthRequest auth);
    }

    public class FEXAuthRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public long Cuit { get; set; }
    }

    public class FEXDummyResponse
    {
        public string AppServer { get; set; } = string.Empty;
        public string DbServer { get; set; } = string.Empty;
        public string AuthServer { get; set; } = string.Empty;
    }

    public class FEXLastCMPResponse
    {
        public long CbteNro { get; set; }
        public FEXError[]? Errors { get; set; }
    }

    public class FEXRequest
    {
        public FEXCabRequest FeCabReq { get; set; } = new FEXCabRequest();
        public FEXDetRequest[] FeDetReq { get; set; } = new FEXDetRequest[0];
    }

    public class FEXCabRequest
    {
        public int CantReg { get; set; }
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
    }

    public class FEXDetRequest
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public long CbteDesde { get; set; }
        public long CbteHasta { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public decimal ImpTotal { get; set; }
        public decimal ImpTotConc { get; set; }
        public decimal ImpNeto { get; set; }
        public decimal ImpOpEx { get; set; }
        public decimal ImpTrib { get; set; }
        public decimal ImpIVA { get; set; }
        public string MonId { get; set; } = string.Empty;
        public decimal MonCotiz { get; set; }
        public string FchServDesde { get; set; } = string.Empty;
        public string FchServHasta { get; set; } = string.Empty;
        public string FchVtoPago { get; set; } = string.Empty;
        public FEXAlicIva[] Iva { get; set; } = new FEXAlicIva[0];
        public FEXTributo[] Tributos { get; set; } = new FEXTributo[0];
        public FEXCbteAsoc[] CbtesAsoc { get; set; } = new FEXCbteAsoc[0];
        public FEXOpcional[] Opcionales { get; set; } = new FEXOpcional[0];
    }

    public class FEXResponse
    {
        public FEXCabResponse? FeCabResp { get; set; }
        public FEXDetResponse[] FeDetResp { get; set; } = new FEXDetResponse[0];
        public FEXError[]? Errors { get; set; }
    }

    public class FEXCabResponse
    {
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
    }

    public class FEXDetResponse
    {
        public long CbteDesde { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public string CAE { get; set; } = string.Empty;
        public string CAEFchVto { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public FEXObs[]? Observaciones { get; set; }
    }

    public class FEXAlicIva
    {
        public int Id { get; set; }
        public decimal BaseImp { get; set; }
        public decimal Importe { get; set; }
    }

    public class FEXTributo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public decimal BaseImp { get; set; }
        public decimal Alic { get; set; }
        public decimal Importe { get; set; }
    }

    public class FEXCbteAsoc
    {
        public int Tipo { get; set; }
        public int PtoVta { get; set; }
        public long Nro { get; set; }
    }

    public class FEXOpcional
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
    }

    public class FEXObs
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
    }

    public class FEXError
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
    }

    public class FEXParamResponse
    {
        public FEXParamItem[] ResultGet { get; set; } = new FEXParamItem[0];
        public FEXError[]? Errors { get; set; }
    }

    public class FEXParamItem
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string FchDesde { get; set; } = string.Empty;
        public string FchHasta { get; set; } = string.Empty;
    }
} 