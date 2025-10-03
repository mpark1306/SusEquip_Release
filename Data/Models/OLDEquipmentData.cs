namespace SusEquip.Data.Models
{
    /// <summary>
    /// OLD Equipment data model for legacy equipment records.
    /// Inherits common properties from BaseEquipmentData and implements Inst_No as string.
    /// </summary>
    public class OLDEquipmentData : BaseEquipmentData
    {
        /// <summary>
        /// Equipment's Instance Number as string for OLD equipment records.
        /// </summary>
        public string Inst_No { get; set; } = string.Empty;

        /// <summary>
        /// Implementation of abstract method to get Inst_No as string.
        /// </summary>
        public override string GetInstNoAsString()
        {
            return Inst_No ?? string.Empty;
        }

        /// <summary>
        /// Override to provide OLD equipment-specific display formatting.
        /// </summary>
        public override string GetDisplayName()
        {
            return $"{PC_Name} (OLD-{Inst_No})";
        }

        /// <summary>
        /// Override to provide OLD equipment-specific validation.
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrWhiteSpace(Inst_No);
        }
    }
}
