namespace Afip.Dotnet.Abstractions.Services
{
    /// <summary>
    /// Main client interface for accessing all AFIP/ARCA web services
    /// </summary>
    public interface IAfipClient
    {
        /// <summary>
        /// WSAA authentication service
        /// </summary>
        IWsaaService Authentication { get; }
        
        /// <summary>
        /// WSFEv1 electronic invoicing service
        /// </summary>
        IWsfev1Service ElectronicInvoicing { get; }
        
        /// <summary>
        /// WSFEX export invoicing service
        /// </summary>
        IWsfexService ExportInvoicing { get; }
        
        /// <summary>
        /// WSMTXCA detailed invoicing service
        /// </summary>
        IWsmtxcaService DetailedInvoicing { get; }
        
        /// <summary>
        /// AFIP parameters service
        /// </summary>
        IAfipParametersService Parameters { get; }
    }
}