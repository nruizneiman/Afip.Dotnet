using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Afip.Dotnet.Abstractions.Models;
using Afip.Dotnet.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Afip.Dotnet.Services
{
    /// <summary>
    /// Implementation of WSAA (Web Service Authentication and Authorization) service
    /// </summary>
    public class WsaaService : IWsaaService
    {
        private readonly AfipConfiguration _configuration;
        private readonly ILogger<WsaaService>? _logger;
        private readonly ConcurrentDictionary<string, AfipAuthTicket> _ticketCache = new ConcurrentDictionary<string, AfipAuthTicket>();

        public WsaaService(AfipConfiguration configuration, ILogger<WsaaService>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<AfipAuthTicket> AuthenticateAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Starting authentication for service: {ServiceName}", serviceName);

                // Generate login ticket request
                var loginTicketRequest = GenerateLoginTicketRequest(serviceName);
                
                // Sign the request
                var signedRequest = SignRequest(loginTicketRequest);
                
                // Call WSAA service
                var response = await CallWsaaServiceAsync(signedRequest, cancellationToken);
                
                // Parse response
                var ticket = ParseAuthResponse(response);
                
                // Cache the ticket
                _ticketCache[serviceName] = ticket;
                
                _logger?.LogInformation("Authentication successful for service: {ServiceName}", serviceName);
                return ticket;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Authentication failed for service: {ServiceName}", serviceName);
                throw new AfipException($"Authentication failed for service {serviceName}", ex);
            }
        }

        public async Task<AfipAuthTicket> GetValidTicketAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            if (_ticketCache.TryGetValue(serviceName, out var cachedTicket) && cachedTicket.IsValid)
            {
                _logger?.LogDebug("Using cached ticket for service: {ServiceName}", serviceName);
                return cachedTicket;
            }

            _logger?.LogDebug("Cached ticket not found or expired, authenticating for service: {ServiceName}", serviceName);
            return await AuthenticateAsync(serviceName, cancellationToken);
        }

        public void ClearTicketCache()
        {
            _ticketCache.Clear();
            _logger?.LogInformation("Ticket cache cleared");
        }

        private string GenerateLoginTicketRequest(string serviceName)
        {
            var now = DateTime.UtcNow;
            var generationTime = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var expirationTime = now.AddHours(12).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var uniqueId = Guid.NewGuid().ToString();

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<loginTicketRequest version=""1.0"">
    <header>
        <source>{_configuration.Cuit}</source>
        <destination>cn=wsaa,o=afip,c=ar,serialNumber=CUIT 33693450239</destination>
        <uniqueId>{uniqueId}</uniqueId>
        <generationTime>{generationTime}</generationTime>
        <expirationTime>{expirationTime}</expirationTime>
    </header>
    <service>{serviceName}</service>
</loginTicketRequest>";
        }

        private string SignRequest(string request)
        {
            // Load certificate
            var certificate = LoadCertificate();
            
            // Create XML document
            var doc = new XmlDocument { PreserveWhitespace = false };
            doc.LoadXml(request);

            // Sign the document
            var signedXml = new SignedXml(doc)
            {
                SigningKey = certificate.GetRSAPrivateKey()
            };

            var reference = new Reference
            {
                Uri = "",
                DigestMethod = SignedXml.XmlDsigSHA1Url
            };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            var signatureElement = signedXml.GetXml();
            doc.DocumentElement.AppendChild(doc.ImportNode(signatureElement, true));

            return doc.OuterXml;
        }

        private async Task<string> CallWsaaServiceAsync(string signedRequest, CancellationToken cancellationToken)
        {
            var binding = new BasicHttpBinding
            {
                Security = { Mode = BasicHttpSecurityMode.Transport },
                MaxReceivedMessageSize = 1024 * 1024 // 1MB
            };

            var endpoint = new EndpointAddress(_configuration.GetWsaaUrl());
            var factory = new ChannelFactory<IWsaaServiceChannel>(binding, endpoint);

            try
            {
                var channel = factory.CreateChannel();
                var response = await Task.Run(() => channel.LoginCms(signedRequest), cancellationToken);
                return response;
            }
            finally
            {
                factory.Close();
            }
        }

        private AfipAuthTicket ParseAuthResponse(string response)
        {
            var doc = new XmlDocument();
            doc.LoadXml(response);

            var credentials = doc.SelectSingleNode("//credentials");
            if (credentials == null)
                throw new AfipException("Invalid WSAA response: credentials node not found");

            var token = credentials.SelectSingleNode("token")?.InnerText;
            var sign = credentials.SelectSingleNode("sign")?.InnerText;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sign))
                throw new AfipException("Invalid WSAA response: token or sign not found");

            return new AfipAuthTicket
            {
                Token = token,
                Sign = sign,
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            };
        }

        private X509Certificate2 LoadCertificate()
        {
            try
            {
                return new X509Certificate2(_configuration.CertificatePath, _configuration.CertificatePassword);
            }
            catch (Exception ex)
            {
                throw new AfipException($"Failed to load certificate from {_configuration.CertificatePath}", ex);
            }
        }
    }

    [ServiceContract]
    internal interface IWsaaServiceChannel
    {
        [OperationContract(Action = "http://ar.gov.afip.dif.FEV1/loginCms")]
        string LoginCms(string request);
    }
}