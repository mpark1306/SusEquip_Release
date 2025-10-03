using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Factories.Abstract
{
    /// <summary>
    /// Factory for server equipment family
    /// Creates related server, backup, and monitoring equipment
    /// </summary>
    public class ServerEquipmentFactory : BaseEquipmentFamilyFactory
    {
        public ServerEquipmentFactory(ILogger<ServerEquipmentFactory> logger) 
            : base(logger, new EquipmentFamilyConfig
            {
                FamilyType = "Server",
                DefaultStatus = "Active",
                NamingPattern = "SRV"
            })
        {
        }

        public override BaseEquipmentData CreatePrimaryEquipment()
        {
            var equipment = new EquipmentData
            {
                App_Owner = "IT Department",
                Serial_No = $"SRV-{System.DateTime.Now:yyyyMMddHHmmss}",
                MachineType = "Server"
            };

            ApplyFamilyConfiguration(equipment, "PRIMARY");
            return equipment;
        }

        public override BaseEquipmentData CreateBackupEquipment()
        {
            var equipment = new EquipmentData
            {
                App_Owner = "IT Department",
                Serial_No = $"BKSRV-{System.DateTime.Now:yyyyMMddHHmmss}",
                MachineType = "Backup Server"
            };

            ApplyFamilyConfiguration(equipment, "BACKUP");
            return equipment;
        }

        public override BaseEquipmentData CreateMonitoringEquipment()
        {
            var equipment = new EquipmentData
            {
                App_Owner = "IT Department",
                Serial_No = $"MON-{System.DateTime.Now:yyyyMMddHHmmss}",
                MachineType = "Monitoring Device"
            };

            ApplyFamilyConfiguration(equipment, "MONITORING");
            return equipment;
        }

        protected override void ApplyFamilyConfiguration(BaseEquipmentData equipment, string role)
        {
            base.ApplyFamilyConfiguration(equipment, role);
            
            // Apply server-specific configuration
            equipment.Department = "IT Operations";
            equipment.Status = "Active";
            equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            equipment.Creator_Initials = "FACTORY";
            
            // Role-specific configuration
            switch (role.ToUpperInvariant())
            {
                case "PRIMARY":
                    equipment.PC_Name = $"SRV-PRIMARY-{DateTime.Now:HHmmss}";
                    break;
                case "BACKUP":
                    equipment.PC_Name = $"SRV-BACKUP-{DateTime.Now:HHmmss}";
                    break;
                case "MONITORING":
                    equipment.PC_Name = $"SRV-MONITOR-{DateTime.Now:HHmmss}";
                    break;
            }
        }
    }
}