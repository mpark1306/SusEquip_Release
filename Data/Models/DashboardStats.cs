namespace SusEquip.Data.Models
{
    /// <summary>
    /// Model for dashboard statistics data
    /// </summary>
    public class DashboardStats
    {
        public int ActiveCount { get; set; }
        public int NewCount { get; set; }
        public int UsedCount { get; set; }
        public int QuarantinedCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}