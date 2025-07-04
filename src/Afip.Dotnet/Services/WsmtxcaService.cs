using System;
using System.Collections.Generic;
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
    /// Implementation of WSMTXCA service for detailed invoicing with item details
    /// </summary>
    public class WsmtxcaService : IWsmtxcaService
    {
        private readonly IWsaaService _wsaaService;
        private readonly IAfipParametersService _parametersService;
        private readonly ILogger<WsmtxcaService> _logger;
        private readonly AfipConfiguration _configuration;

        public WsmtxcaService(
            IWsaaService wsaaService,
            IAfipParametersService parametersService,
            ILogger<WsmtxcaService> logger,
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
                _logger.LogInformation("Checking WSMTXCA service status");

                var channel = CreateServiceChannel();
                var response = await Task.Run(() => channel.FEXDummy(), cancellationToken);

                return new ServiceStatus
                {
                    AppServer = response.AppServer,
                    DbServer = response.DbServer,
                    AuthServer = response.AuthServer,
                    StatusMessage = "WSMTXCA service is available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking WSMTXCA service status");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetLastInvoiceNumberAsync(int pointOfSale, int invoiceType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting last invoice number for point of sale {PointOfSale} and invoice type {InvoiceType}", pointOfSale, invoiceType);

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetLast_CMP(authInfo, pointOfSale, invoiceType), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA error: {error.Msg} (Code: {error.Code})");
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
        public async Task<DetailedInvoiceResponse> AuthorizeDetailedInvoiceAsync(DetailedInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Authorizing detailed invoice for point of sale {PointOfSale} and invoice type {InvoiceType}", request.PointOfSale, request.InvoiceType);

                ValidateDetailedInvoiceRequest(request);

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var mtxcRequest = MapToMtxcRequest(request);
                var response = await Task.Run(() => channel.FEXAuthorize(authInfo, mtxcRequest), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA authorization error: {error.Msg} (Code: {error.Code})");
                }

                return MapToDetailedInvoiceResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing detailed invoice for point of sale {PointOfSale} and invoice type {InvoiceType}", request.PointOfSale, request.InvoiceType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DetailedInvoiceResponse?> QueryDetailedInvoiceAsync(int pointOfSale, int invoiceType, long invoiceNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Querying detailed invoice for point of sale {PointOfSale}, invoice type {InvoiceType}, and invoice number {InvoiceNumber}", pointOfSale, invoiceType, invoiceNumber);

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
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
                    
                    throw new AfipException($"WSMTXCA query error: {error.Msg} (Code: {error.Code})");
                }

                return MapToDetailedInvoiceResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying detailed invoice for point of sale {PointOfSale}, invoice type {InvoiceType}, and invoice number {InvoiceNumber}", pointOfSale, invoiceType, invoiceNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetDetailedInvoiceTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting detailed invoice types");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Cbte(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed invoice types");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetDetailedDocumentTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting detailed document types");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Doc(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed document types");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetDetailedVatConditionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting detailed VAT conditions");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Iva(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed VAT conditions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetDetailedVatRatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting detailed VAT rates");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Iva(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed VAT rates");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetDetailedUnitsOfMeasurementAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting detailed units of measurement");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_UMed(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed units of measurement");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetDetailedItemCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting detailed item categories");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Concepto(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed item categories");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetItemTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting item types");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Concepto(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item types");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ParameterItem>> GetTaxTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax types");

                var authTicket = await _wsaaService.GetValidTicketAsync("wsmtxca", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new MTXCAuthRequest
                {
                    Token = authTicket.Token,
                    Sign = authTicket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEXGetPARAM_Tipo_Tributo(authInfo), cancellationToken);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    var error = response.Errors[0];
                    throw new AfipException($"WSMTXCA parameter error: {error.Msg} (Code: {error.Code})");
                }

                return MapToParameterItems(response.ResultGet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax types");
                throw;
            }
        }

        private void ValidateDetailedInvoiceRequest(DetailedInvoiceRequest request)
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

            if (request.Items == null || request.Items.Count == 0)
                throw new ArgumentException("At least one item is required", nameof(request.Items));

            if (request.TotalAmount <= 0)
                throw new ArgumentException("Total amount must be greater than 0", nameof(request.TotalAmount));
        }

        private IWsmtxcaServiceChannel CreateServiceChannel()
        {
            var binding = new BasicHttpBinding
            {
                Security = { Mode = BasicHttpSecurityMode.Transport },
                MaxReceivedMessageSize = 1024 * 1024 // 1MB
            };

            var endpoint = new EndpointAddress(_configuration.GetWsmtxcaUrl());
            var factory = new ChannelFactory<IWsmtxcaServiceChannel>(binding, endpoint);

            return factory.CreateChannel();
        }

        private MTXCRequest MapToMtxcRequest(DetailedInvoiceRequest request)
        {
            return new MTXCRequest
            {
                FeCabReq = new MTXCCabRequest
                {
                    CantReg = 1,
                    PtoVta = request.PointOfSale,
                    CbteTipo = request.InvoiceType
                },
                FeDetReq = new MTXCDetRequest[]
                {
                    new MTXCDetRequest
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
                        Iva = request.VatDetails?.Select(v => new MTXCAlicIva
                        {
                            Id = v.VatId,
                            BaseImp = v.BaseAmount,
                            Importe = v.Amount
                        }).ToArray() ?? new MTXCAlicIva[0],
                        Tributos = request.TaxDetails?.Select(t => new MTXCTributo
                        {
                            Id = t.TaxId,
                            Desc = t.Description,
                            BaseImp = t.BaseAmount,
                            Alic = t.Rate,
                            Importe = t.Amount
                        }).ToArray() ?? new MTXCTributo[0],
                        CbtesAsoc = request.AssociatedInvoices?.Select(a => new MTXCCbteAsoc
                        {
                            Tipo = a.InvoiceType,
                            PtoVta = a.PointOfSale,
                            Nro = a.InvoiceNumber
                        }).ToArray() ?? new MTXCCbteAsoc[0],
                        Opcionales = request.OptionalData?.Select(o => new MTXCOpcional
                        {
                            Id = o.Id,
                            Valor = o.Value
                        }).ToArray() ?? new MTXCOpcional[0],
                        Items = request.Items?.Select(i => new MTXCItem
                        {
                            Desc = i.Description,
                            Qty = i.Quantity,
                            PrecioUnit = i.UnitPrice,
                            ImpTotal = i.TotalAmount,
                            ImpNeto = i.NetAmount,
                            ImpIVA = i.VatAmount,
                            UMed = i.UnitOfMeasurement,
                            Categoria = i.ItemCategory,
                            Tipo = i.ItemType,
                            Iva = i.VatRate,
                            Tributos = i.Taxes?.Select(t => new MTXCItemTax
                            {
                                Id = t.TaxId,
                                Desc = t.Description,
                                BaseImp = t.BaseAmount,
                                Alic = t.TaxRate,
                                Importe = t.TaxAmount
                            }).ToArray() ?? new MTXCItemTax[0],
                            Descuentos = i.Discounts?.Select(d => new MTXCItemDiscount
                            {
                                Id = d.DiscountType,
                                Desc = d.Description,
                                Porcentaje = d.DiscountRate,
                                Importe = d.DiscountAmount,
                                BaseImp = d.BaseAmount
                            }).ToArray() ?? new MTXCItemDiscount[0],
                            Opcionales = i.OptionalData?.Select(o => new MTXCOpcional
                            {
                                Id = o.Id,
                                Valor = o.Value
                            }).ToArray() ?? new MTXCOpcional[0]
                        }).ToArray() ?? new MTXCItem[0]
                    }
                }
            };
        }

        private DetailedInvoiceResponse MapToDetailedInvoiceResponse(MTXCResponse response)
        {
            if (response.FeDetResp == null || response.FeDetResp.Length == 0)
                throw new AfipException("Invalid WSMTXCA response: no invoice details");

            var detResponse = response.FeDetResp[0];
            
            return new DetailedInvoiceResponse
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
                }).ToList() ?? new List<Observation>(),
                Items = detResponse.Items?.Select(i => new InvoiceItem
                {
                    Description = i.Desc,
                    Quantity = i.Qty,
                    UnitPrice = i.PrecioUnit,
                    TotalAmount = i.ImpTotal,
                    NetAmount = i.ImpNeto,
                    VatAmount = i.ImpIVA,
                    UnitOfMeasurement = i.UMed,
                    ItemCategory = i.Categoria,
                    ItemType = i.Tipo,
                    VatRate = i.Iva,
                    Taxes = i.Tributos?.Select(t => new ItemTax
                    {
                        TaxId = t.Id,
                        Description = t.Desc,
                        BaseAmount = t.BaseImp,
                        TaxRate = t.Alic,
                        TaxAmount = t.Importe
                    }).ToList() ?? new List<ItemTax>(),
                    Discounts = i.Descuentos?.Select(d => new ItemDiscount
                    {
                        DiscountType = d.Id,
                        Description = d.Desc,
                        DiscountRate = d.Porcentaje,
                        DiscountAmount = d.Importe,
                        BaseAmount = d.BaseImp
                    }).ToList() ?? new List<ItemDiscount>(),
                    OptionalData = i.Opcionales?.Select(o => new OptionalData
                    {
                        Id = o.Id,
                        Value = o.Valor
                    }).ToList() ?? new List<OptionalData>()
                }).ToList() ?? new List<InvoiceItem>()
            };
        }

        private List<ParameterItem> MapToParameterItems(MTXCParamItem[] items)
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

    // WSMTXCA SOAP service contracts and data contracts
    [ServiceContract]
    internal interface IWsmtxcaServiceChannel
    {
        [OperationContract]
        MTXCDummyResponse FEXDummy();
        
        [OperationContract]
        MTXCLastCMPResponse FEXGetLast_CMP(MTXCAuthRequest auth, int ptoVta, int cbteTipo);
        
        [OperationContract]
        MTXCResponse FEXAuthorize(MTXCAuthRequest auth, MTXCRequest request);
        
        [OperationContract]
        MTXCResponse FEXGet_CMP(MTXCAuthRequest auth, int ptoVta, int cbteTipo, long cbteNro);
        
        [OperationContract]
        MTXCParamResponse FEXGetPARAM_Tipo_Cbte(MTXCAuthRequest auth);
        
        [OperationContract]
        MTXCParamResponse FEXGetPARAM_Tipo_Doc(MTXCAuthRequest auth);
        
        [OperationContract]
        MTXCParamResponse FEXGetPARAM_Tipo_Iva(MTXCAuthRequest auth);
        
        [OperationContract]
        MTXCParamResponse FEXGetPARAM_UMed(MTXCAuthRequest auth);
        
        [OperationContract]
        MTXCParamResponse FEXGetPARAM_Tipo_Concepto(MTXCAuthRequest auth);
        
        [OperationContract]
        MTXCParamResponse FEXGetPARAM_Tipo_Tributo(MTXCAuthRequest auth);
    }

    public class MTXCAuthRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public long Cuit { get; set; }
    }

    public class MTXCDummyResponse
    {
        public string AppServer { get; set; } = string.Empty;
        public string DbServer { get; set; } = string.Empty;
        public string AuthServer { get; set; } = string.Empty;
    }

    public class MTXCLastCMPResponse
    {
        public long CbteNro { get; set; }
        public MTXCError[]? Errors { get; set; }
    }

    public class MTXCRequest
    {
        public MTXCCabRequest FeCabReq { get; set; } = new MTXCCabRequest();
        public MTXCDetRequest[] FeDetReq { get; set; } = new MTXCDetRequest[0];
    }

    public class MTXCCabRequest
    {
        public int CantReg { get; set; }
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
    }

    public class MTXCDetRequest
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
        public MTXCAlicIva[] Iva { get; set; } = new MTXCAlicIva[0];
        public MTXCTributo[] Tributos { get; set; } = new MTXCTributo[0];
        public MTXCCbteAsoc[] CbtesAsoc { get; set; } = new MTXCCbteAsoc[0];
        public MTXCOpcional[] Opcionales { get; set; } = new MTXCOpcional[0];
        public MTXCItem[] Items { get; set; } = new MTXCItem[0];
    }

    public class MTXCResponse
    {
        public MTXCCabResponse? FeCabResp { get; set; }
        public MTXCDetResponse[] FeDetResp { get; set; } = new MTXCDetResponse[0];
        public MTXCError[]? Errors { get; set; }
    }

    public class MTXCCabResponse
    {
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
    }

    public class MTXCDetResponse
    {
        public long CbteDesde { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public string CAE { get; set; } = string.Empty;
        public string CAEFchVto { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public MTXCObs[]? Observaciones { get; set; }
        public MTXCItem[]? Items { get; set; }
    }

    public class MTXCItem
    {
        public string Desc { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal ImpTotal { get; set; }
        public decimal ImpNeto { get; set; }
        public decimal ImpIVA { get; set; }
        public int UMed { get; set; }
        public int Categoria { get; set; }
        public int Tipo { get; set; }
        public int Iva { get; set; }
        public MTXCItemTax[] Tributos { get; set; } = new MTXCItemTax[0];
        public MTXCItemDiscount[] Descuentos { get; set; } = new MTXCItemDiscount[0];
        public MTXCOpcional[] Opcionales { get; set; } = new MTXCOpcional[0];
    }

    public class MTXCItemTax
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public decimal BaseImp { get; set; }
        public decimal Alic { get; set; }
        public decimal Importe { get; set; }
    }

    public class MTXCItemDiscount
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public decimal Importe { get; set; }
        public decimal BaseImp { get; set; }
    }

    public class MTXCAlicIva
    {
        public int Id { get; set; }
        public decimal BaseImp { get; set; }
        public decimal Importe { get; set; }
    }

    public class MTXCTributo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public decimal BaseImp { get; set; }
        public decimal Alic { get; set; }
        public decimal Importe { get; set; }
    }

    public class MTXCCbteAsoc
    {
        public int Tipo { get; set; }
        public int PtoVta { get; set; }
        public long Nro { get; set; }
    }

    public class MTXCOpcional
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
    }

    public class MTXCObs
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
    }

    public class MTXCError
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
    }

    public class MTXCParamResponse
    {
        public MTXCParamItem[] ResultGet { get; set; } = new MTXCParamItem[0];
        public MTXCError[]? Errors { get; set; }
    }

    public class MTXCParamItem
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string FchDesde { get; set; } = string.Empty;
        public string FchHasta { get; set; } = string.Empty;
    }
} 