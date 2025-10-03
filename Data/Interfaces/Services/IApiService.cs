using SusEquip.Data.Models;

namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for API service operations
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Gets authentication token for API access
        /// </summary>
        LoginTokenResult GetToken();
        
        /// <summary>
        /// Refreshes the API token
        /// </summary>
        void RefreshApiToken();
        
        /// <summary>
        /// Gets authentication token asynchronously
        /// </summary>
        Task<LoginTokenResult> GetTokenAsync();
        
        /// <summary>
        /// Refreshes the API token asynchronously
        /// </summary>
        Task RefreshApiTokenAsync();
        
        /// <summary>
        /// Checks if the current token is valid
        /// </summary>
        Task<bool> IsTokenValidAsync();
    }
}