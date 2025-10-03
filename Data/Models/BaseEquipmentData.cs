using System.ComponentModel.DataAnnotations;

namespace SusEquip.Data.Models
{
    /// <summary>
    /// Abstract base class containing common properties shared between EquipmentData and OLDEquipmentData.
    /// Eliminates code duplication while maintaining flexibility through virtual methods.
    /// </summary>
    public abstract class BaseEquipmentData
    {
        [Key]
        public int EntryId { get; set; }

        public string Entry_Date { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PC_Name { get; set; } = string.Empty;

        public string Creator_Initials { get; set; } = string.Empty;

        public string App_Owner { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [StringLength(50)]
        public string Serial_No { get; set; } = string.Empty;

        public string Mac_Address1 { get; set; } = string.Empty;

        public string Mac_Address2 { get; set; } = string.Empty;

        public string UUID { get; set; } = string.Empty;

        public string Product_No { get; set; } = string.Empty;

        public string Model_Name_and_No { get; set; } = string.Empty;

        public string Service_Start { get; set; } = string.Empty;

        public string Service_Ends { get; set; } = string.Empty;

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        [StringLength(50)]
        public string MachineType { get; set; } = string.Empty;

        /// <summary>
        /// Virtual method to get Inst_No as string for display purposes.
        /// Must be overridden in derived classes to handle different data types.
        /// </summary>
        public abstract string GetInstNoAsString();

        /// <summary>
        /// Virtual method to validate the equipment data.
        /// Can be overridden in derived classes for specific validation rules.
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PC_Name) && 
                   !string.IsNullOrWhiteSpace(GetInstNoAsString());
        }

        /// <summary>
        /// Virtual method to get a display name for the equipment.
        /// Can be overridden in derived classes for custom formatting.
        /// </summary>
        public virtual string GetDisplayName()
        {
            return $"{PC_Name} ({GetInstNoAsString()})";
        }

        /// <summary>
        /// Virtual method to check if equipment is active.
        /// Can be overridden in derived classes for specific business logic.
        /// </summary>
        public virtual bool IsActive()
        {
            return !string.Equals(Status, "Kasseret", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(Status, "Expired", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Virtual method to get formatted service period.
        /// </summary>
        public virtual string GetServicePeriod()
        {
            if (!string.IsNullOrEmpty(Service_Start) && !string.IsNullOrEmpty(Service_Ends))
            {
                return $"{Service_Start} - {Service_Ends}";
            }
            return string.Empty;
        }

        /// <summary>
        /// Virtual method to check if service is active based on dates.
        /// </summary>
        public virtual bool IsServiceActive()
        {
            if (DateTime.TryParse(Service_Ends, out DateTime endDate))
            {
                return endDate > DateTime.Now;
            }
            return false;
        }
    }
}