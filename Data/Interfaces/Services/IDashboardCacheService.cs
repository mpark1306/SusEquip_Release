using SusEquip.Data.Models;

namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for dashboard caching operations
    /// </summary>
    public interface IDashboardCacheService
    {
        /// <summary>
        /// Gets dashboard statistics from cache or computes them
        /// </summary>
        Task<DashboardStats> GetDashboardStatsAsync();
        
        /// <summary>
        /// Refreshes dashboard statistics cache
        /// </summary>
        Task<DashboardStats> RefreshDashboardStatsAsync();
        
        /// <summary>
        /// Clears dashboard cache
        /// </summary>
        void ClearCache();
    }
}