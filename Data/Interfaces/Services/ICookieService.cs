namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for cookie management operations
    /// </summary>
    public interface ICookieService
    {
        /// <summary>
        /// Gets a cookie value by name asynchronously
        /// </summary>
        Task<string> GetCookieAsync(string key);
        
        /// <summary>
        /// Sets a cookie with specified name, value, and optional expiration time
        /// </summary>
        Task SetCookieAsync(string key, string value, int? expireTime);
    }
}