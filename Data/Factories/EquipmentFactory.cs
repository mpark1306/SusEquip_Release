using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using System;

namespace SusEquip.Data.Factories
{
    /// <summary>
    /// Basic factory interface for equipment creation
    /// </summary>
    public interface IEquipmentFactory
    {
        BaseEquipmentData CreateEquipment(string type);
        BaseEquipmentData CreateFromExisting(BaseEquipmentData source, string targetType);
        T CreateEquipment<T>() where T : BaseEquipmentData, new();
        IEnumerable<string> GetSupportedTypes();
        bool SupportsType(string type);
    }

    /// <summary>
    /// Basic equipment factory implementation
    /// </summary>
    public class EquipmentFactory : IEquipmentFactory
    {
        private readonly ILogger<EquipmentFactory> _logger;

        public EquipmentFactory(ILogger<EquipmentFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public BaseEquipmentData CreateEquipment(string type)
        {
            return type.ToUpper() switch
            {
                "STANDARD" => new EquipmentData(),
                "OLD" => new OLDEquipmentData(),
                "MACHINE" => new MachineData(),
                _ => throw new ArgumentException($"Unknown equipment type: {type}", nameof(type))
            };
        }

        public BaseEquipmentData CreateFromExisting(BaseEquipmentData source, string targetType)
        {
            var target = CreateEquipment(targetType);
            
            // Copy common properties
            target.Entry_Date = source.Entry_Date;
            target.PC_Name = source.PC_Name;
            target.Creator_Initials = source.Creator_Initials;
            target.App_Owner = source.App_Owner;
            target.Status = source.Status;
            target.Serial_No = source.Serial_No;
            target.Mac_Address1 = source.Mac_Address1;
            target.Mac_Address2 = source.Mac_Address2;
            target.UUID = source.UUID;
            target.Product_No = source.Product_No;
            target.Model_Name_and_No = source.Model_Name_and_No;
            target.Service_Start = source.Service_Start;
            target.Service_Ends = source.Service_Ends;
            target.Department = source.Department;
            target.Note = source.Note;
            target.MachineType = source.MachineType;

            // Handle type-specific properties
            switch (target)
            {
                case EquipmentData equipData when source is OLDEquipmentData oldSource:
                    if (int.TryParse(oldSource.Inst_No, out int instNo))
                        equipData.Inst_No = instNo;
                    break;
                case OLDEquipmentData oldData when source is EquipmentData equipSource:
                    oldData.Inst_No = equipSource.Inst_No.ToString();
                    break;
            }

            _logger.LogDebug("Converted {SourceType} to {TargetType} for equipment {PCName}", 
                source.GetType().Name, target.GetType().Name, source.PC_Name);

            return target;
        }

        public T CreateEquipment<T>() where T : BaseEquipmentData, new()
        {
            return new T();
        }

        public IEnumerable<string> GetSupportedTypes()
        {
            return new[] { "STANDARD", "OLD", "MACHINE" };
        }

        public bool SupportsType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return false;
                
            return type.ToUpper() switch
            {
                "STANDARD" or "OLD" or "MACHINE" => true,
                _ => false
            };
        }
    }
}