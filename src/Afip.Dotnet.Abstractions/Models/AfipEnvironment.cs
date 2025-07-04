namespace Afip.Dotnet.Abstractions.Models
{
    /// <summary>
    /// Represents the AFIP/ARCA environment to use for web service calls
    /// </summary>
    public enum AfipEnvironment
    {
        /// <summary>
        /// Testing/Homologation environment for development and testing
        /// </summary>
        Testing = 0,
        
        /// <summary>
        /// Production environment for live operations
        /// </summary>
        Production = 1
    }
}