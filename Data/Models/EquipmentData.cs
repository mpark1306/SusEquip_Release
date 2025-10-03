namespace SusEquip.Data.Models
{
    /// <summary>
    /// Equipment data model for active equipment records.
    /// Inherits common properties from BaseEquipmentData and implements Inst_No as int.
    /// </summary>
    public class EquipmentData : BaseEquipmentData
    {
        /// <summary>
        /// Equipment's Instance Number as int.
        /// </summary>
        public int Inst_No { get; set; }

        /// <summary>
        /// Implementation of abstract method to get Inst_No as string.
        /// </summary>
        public override string GetInstNoAsString()
        {
            return Inst_No.ToString();
        }

        /// <summary>
        /// Override to provide equipment-specific display formatting.
        /// </summary>
        public override string GetDisplayName()
        {
            return $"{PC_Name} (#{Inst_No})";
        }

        /// <summary>
        /// Override to provide equipment-specific validation.
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() && Inst_No > 0;
        }
    }
}
