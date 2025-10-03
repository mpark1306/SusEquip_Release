using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Factories.Abstract
{
    /// <summary>
    /// Abstract factory interface for creating equipment families
    /// Supports creation of related equipment objects with consistent configuration
    /// </summary>
    public interface IEquipmentFamilyFactory
    {
        /// <summary>
        /// Creates the primary equipment for this family
        /// </summary>
        BaseEquipmentData CreatePrimaryEquipment();
        
        /// <summary>
        /// Creates a backup/secondary equipment for this family
        /// </summary>
        BaseEquipmentData CreateBackupEquipment();
        
        /// <summary>
        /// Creates monitoring equipment for this family
        /// </summary>
        BaseEquipmentData CreateMonitoringEquipment();
        
        /// <summary>
        /// Gets the family type identifier
        /// </summary>
        string FamilyType { get; }
        
        /// <summary>
        /// Gets the family configuration settings
        /// </summary>
        EquipmentFamilyConfig GetFamilyConfig();
    }

    /// <summary>
    /// Configuration settings for equipment families
    /// </summary>
    public class EquipmentFamilyConfig
    {
        public string FamilyType { get; set; } = string.Empty;
        public string DefaultStatus { get; set; } = "Active";
        public string DefaultLocation { get; set; } = string.Empty;
        public bool RequiresBackup { get; set; } = false;
        public bool RequiresMonitoring { get; set; } = false;
        public string NamingPattern { get; set; } = string.Empty;
    }
}