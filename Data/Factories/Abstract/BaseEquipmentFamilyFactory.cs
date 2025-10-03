using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;
using System;

namespace SusEquip.Data.Factories.Abstract
{
    /// <summary>
    /// Base implementation for equipment family factories
    /// Provides common functionality for all equipment families
    /// </summary>
    public abstract class BaseEquipmentFamilyFactory : IEquipmentFamilyFactory
    {
        protected readonly ILogger _logger;
        protected readonly EquipmentFamilyConfig _config;

        protected BaseEquipmentFamilyFactory(ILogger logger, EquipmentFamilyConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public abstract BaseEquipmentData CreatePrimaryEquipment();
        public abstract BaseEquipmentData CreateBackupEquipment();
        public abstract BaseEquipmentData CreateMonitoringEquipment();

        public virtual string FamilyType => _config.FamilyType;

        public virtual EquipmentFamilyConfig GetFamilyConfig() => _config;

        /// <summary>
        /// Applies common family configuration to equipment
        /// </summary>
        protected virtual void ApplyFamilyConfiguration(BaseEquipmentData equipment, string role)
        {
            if (equipment == null) return;

            equipment.Status = _config.DefaultStatus;
            equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            equipment.Creator_Initials = "System";

            // Apply naming pattern if specified
            if (!string.IsNullOrEmpty(_config.NamingPattern))
            {
                equipment.PC_Name = $"{_config.NamingPattern}-{role}-{DateTime.Now:yyyyMMdd}";
            }

            _logger.LogInformation($"Created {role} equipment for family {FamilyType}");
        }
    }
}