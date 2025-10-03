using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Factories.Abstract
{
    /// <summary>
    /// Factory for workstation equipment family
    /// Creates related workstation, backup, and monitoring equipment
    /// </summary>
    public class WorkstationEquipmentFactory : BaseEquipmentFamilyFactory
    {
        public WorkstationEquipmentFactory(ILogger<WorkstationEquipmentFactory> logger) 
            : base(logger, new EquipmentFamilyConfig
            {
                FamilyType = "WORKSTATION",
                DefaultStatus = "Active",
                DefaultLocation = "Office",
                RequiresBackup = false,
                RequiresMonitoring = false,
                NamingPattern = "WS"
            })
        {
        }

        public override BaseEquipmentData CreatePrimaryEquipment()
        {
            var equipment = new EquipmentData
            {
                App_Owner = "End User",
                Serial_No = $"WS-{System.DateTime.Now:yyyyMMddHHmmss}",
                MachineType = "Workstation"
            };

            ApplyFamilyConfiguration(equipment, "PRIMARY");
            return equipment;
        }

        public override BaseEquipmentData CreateBackupEquipment()
        {
            var equipment = new EquipmentData
            {
                App_Owner = "End User",
                Serial_No = $"WS-SPARE-{System.DateTime.Now:yyyyMMddHHmmss}",
                MachineType = "Spare Workstation",
                Status = "Standby"
            };

            ApplyFamilyConfiguration(equipment, "SPARE");
            return equipment;
        }

        public override BaseEquipmentData CreateMonitoringEquipment()
        {
            // Workstations typically don't need dedicated monitoring equipment
            // Return a basic monitoring configuration
            var equipment = new EquipmentData
            {
                App_Owner = "IT Department",
                Serial_No = $"WS-MON-{System.DateTime.Now:yyyyMMddHHmmss}",
                MachineType = "Software Monitoring",
                Status = "Monitoring"
            };

            ApplyFamilyConfiguration(equipment, "SOFTWARE");
            return equipment;
        }
    }
}