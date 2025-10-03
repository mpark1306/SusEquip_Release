namespace SusEquip.Data.Models
{
    /// <summary>
    /// Machine-specific equipment data model.
    /// Inherits from EquipmentData and adds machine-specific functionality.
    /// </summary>
    public class MachineData : EquipmentData
    {
        /// <summary>
        /// Override to provide machine-specific display formatting.
        /// </summary>
        public override string GetDisplayName()
        {
            var machineType = !string.IsNullOrEmpty(MachineType) ? $" ({MachineType})" : "";
            return $"{PC_Name}{machineType} [#{Inst_No}]";
        }

        /// <summary>
        /// Checks if this machine is actively in service.
        /// </summary>
        public bool IsInService()
        {
            return IsActive() && 
                   !string.Equals(Status, "New", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(Status, "Quarantine", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if this machine is new/unused.
        /// </summary>
        public bool IsNew()
        {
            return string.Equals(Status, "New", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if this machine is used but available.
        /// </summary>
        public bool IsUsed()
        {
            return string.Equals(Status, "Used", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if this machine is in quarantine.
        /// </summary>
        public bool IsInQuarantine()
        {
            return string.Equals(Status, "Quarantine", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the machine category based on its type.
        /// </summary>
        public string GetMachineCategory()
        {
            if (string.IsNullOrEmpty(MachineType))
                return "Unknown";

            return MachineType.ToUpper() switch
            {
                var type when type.Contains("LAPTOP") => "Portable",
                var type when type.Contains("DESKTOP") => "Desktop",
                var type when type.Contains("SERVER") => "Server",
                var type when type.Contains("TABLET") => "Mobile",
                var type when type.Contains("PHONE") => "Mobile",
                _ => "Other"
            };
        }

        /// <summary>
        /// Gets the age of the machine in days based on service start date.
        /// </summary>
        public int GetAgeInDays()
        {
            if (DateTime.TryParse(Service_Start, out DateTime startDate))
            {
                return (DateTime.Now - startDate).Days;
            }
            return 0;
        }
    }
}
