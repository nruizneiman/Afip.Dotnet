using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Implementation of AFIP parameters service for querying parameter tables
    /// </summary>
    public class AfipParametersService : IAfipParametersService
    {
        private readonly AfipConfiguration _configuration;
        private readonly IWsaaService _wsaaService;
        private readonly ILogger<AfipParametersService>? _logger;

        public AfipParametersService(AfipConfiguration configuration, IWsaaService wsaaService, ILogger<AfipParametersService>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _wsaaService = wsaaService ?? throw new ArgumentNullException(nameof(wsaaService));
            _logger = logger;
        }

        public async Task<List<ParameterItem>> GetInvoiceTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting invoice types from AFIP");

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetTiposCbte(authInfo), cancellationToken);

                return MapToParameterItems(response?.ResultGet);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get invoice types");
                throw new AfipException("Failed to get invoice types", ex);
            }
        }

        public async Task<List<ParameterItem>> GetDocumentTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting document types from AFIP");

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetTiposDoc(authInfo), cancellationToken);

                return MapToParameterItems(response?.ResultGet);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get document types");
                throw new AfipException("Failed to get document types", ex);
            }
        }

        public async Task<List<ParameterItem>> GetConceptTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting concept types from AFIP");

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetTiposConcepto(authInfo), cancellationToken);

                return MapToParameterItems(response?.ResultGet);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get concept types");
                throw new AfipException("Failed to get concept types", ex);
            }
        }

        public async Task<List<ParameterItem>> GetVatRatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting VAT rates from AFIP");

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetTiposIva(authInfo), cancellationToken);

                return MapToParameterItems(response?.ResultGet);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get VAT rates");
                throw new AfipException("Failed to get VAT rates", ex);
            }
        }

        public async Task<List<ParameterItem>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting currencies from AFIP");

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetTiposMonedas(authInfo), cancellationToken);

                return MapToParameterItems(response?.ResultGet);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get currencies");
                throw new AfipException("Failed to get currencies", ex);
            }
        }

        public async Task<List<ParameterItem>> GetTaxTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting tax types from AFIP");

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetTiposTributos(authInfo), cancellationToken);

                return MapToParameterItems(response?.ResultGet);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get tax types");
                throw new AfipException("Failed to get tax types", ex);
            }
        }

        public async Task<decimal> GetCurrencyRateAsync(string currencyId, DateTime date, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting exchange rate for currency {CurrencyId} on {Date}", currencyId, date);

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                var response = await Task.Run(() => channel.FEParamGetCotizacion(authInfo, currencyId), cancellationToken);

                return response?.MonCotiz ?? 1.0m;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get exchange rate for currency {CurrencyId}", currencyId);
                throw new AfipException($"Failed to get exchange rate for currency {currencyId}", ex);
            }
        }

        public async Task<List<ParameterItem>> GetReceiverVatConditionsAsync(string invoiceClass, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Getting receiver VAT conditions for invoice class {InvoiceClass}", invoiceClass);

                var ticket = await _wsaaService.GetValidTicketAsync("wsfe", cancellationToken);
                var channel = CreateServiceChannel();

                var authInfo = new FEAuthRequest
                {
                    Token = ticket.Token,
                    Sign = ticket.Sign,
                    Cuit = _configuration.Cuit
                };

                // This would require additional AFIP service calls specific to VAT conditions
                // For now, returning common VAT conditions after validating the ticket
                var vatConditions = new List<ParameterItem>
                {
                    new ParameterItem { Id = "1", Description = "IVA Responsable Inscripto" },
                    new ParameterItem { Id = "2", Description = "IVA Responsable no Inscripto" },
                    new ParameterItem { Id = "3", Description = "IVA no Responsable" },
                    new ParameterItem { Id = "4", Description = "IVA Sujeto Exento" },
                    new ParameterItem { Id = "5", Description = "Consumidor Final" },
                    new ParameterItem { Id = "6", Description = "Responsable Monotributo" },
                    new ParameterItem { Id = "7", Description = "Sujeto no Categorizado" },
                    new ParameterItem { Id = "8", Description = "Proveedor del Exterior" },
                    new ParameterItem { Id = "9", Description = "Cliente del Exterior" },
                    new ParameterItem { Id = "10", Description = "IVA Liberado - Ley Nº 19.640" },
                    new ParameterItem { Id = "11", Description = "IVA Responsable Inscripto - Agente de Percepción" },
                    new ParameterItem { Id = "12", Description = "Pequeño Contribuyente Eventual" },
                    new ParameterItem { Id = "13", Description = "Monotributista Social" },
                    new ParameterItem { Id = "14", Description = "Pequeño Contribuyente Eventual Social" }
                };

                return vatConditions;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get receiver VAT conditions");
                throw new AfipException("Failed to get receiver VAT conditions", ex);
            }
        }

        private List<ParameterItem> MapToParameterItems(FEParamItem[] items)
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

        private IWsfev1ParametersServiceChannel CreateServiceChannel()
        {
            var binding = new BasicHttpBinding
            {
                Security = { Mode = BasicHttpSecurityMode.Transport },
                MaxReceivedMessageSize = 1024 * 1024 // 1MB
            };

            var endpoint = new EndpointAddress(_configuration.GetWsfev1Url());
            var factory = new ChannelFactory<IWsfev1ParametersServiceChannel>(binding, endpoint);

            return factory.CreateChannel();
        }
    }

    // Additional data contracts for parameter queries
    public class FEParamResponse
    {
        public FEParamItem[] ResultGet { get; set; }
    }

    public class FEParamItem
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public string FchDesde { get; set; }
        public string FchHasta { get; set; }
    }

    public class FECotizacionResponse
    {
        public decimal MonCotiz { get; set; }
    }

    // Extended service contract with parameter methods
    internal interface IWsfev1ParametersServiceChannel
    {
        [OperationContract]
        FEParamResponse FEParamGetTiposCbte(FEAuthRequest auth);
        
        [OperationContract]
        FEParamResponse FEParamGetTiposMonedas(FEAuthRequest auth);
        
        [OperationContract]
        FEParamResponse FEParamGetTiposIva(FEAuthRequest auth);
        
        [OperationContract]
        FEParamResponse FEParamGetTiposDoc(FEAuthRequest auth);
        
        [OperationContract]
        FEParamResponse FEParamGetTiposConcepto(FEAuthRequest auth);
        
        [OperationContract]
        FEParamResponse FEParamGetTiposTributos(FEAuthRequest auth);
        
        [OperationContract]
        FECotizacionResponse FEParamGetCotizacion(FEAuthRequest auth, string monId);
    }
}