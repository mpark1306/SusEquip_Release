namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for caching operations
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache
        /// </summary>
        Task<T?> GetAsync<T>(string key);
        
        /// <summary>
        /// Sets a value in cache with expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        
        /// <summary>
        /// Sets a value in cache with default expiration
        /// </summary>
        Task SetAsync<T>(string key, T value);
        
        /// <summary>
        /// Removes a specific key from cache
        /// </summary>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// Removes all keys matching a pattern
        /// </summary>
        Task RemoveByPatternAsync(string pattern);
        
        /// <summary>
        /// Clears all cache entries
        /// </summary>
        Task ClearAsync();
        
        /// <summary>
        /// Checks if a key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);
        
        /// <summary>
        /// Gets cache statistics
        /// </summary>
        Task<CacheStatistics> GetStatisticsAsync();
    }
    
    /// <summary>
    /// Cache statistics information
    /// </summary>
    public class CacheStatistics
    {
        public int TotalKeys { get; set; }
        public long MemoryUsageBytes { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public double HitRatio => TotalRequests > 0 ? (double)HitCount / TotalRequests * 100 : 0;
        public int TotalRequests => HitCount + MissCount;
    }
}