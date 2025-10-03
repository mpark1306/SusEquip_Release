using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SusEquip.Data.Factories.Methods
{
    /// <summary>
    /// Extensible factory implementation with parameter support
    /// Enables dynamic equipment creation with configuration parameters
    /// </summary>
    public class ExtensibleEquipmentFactory : IExtensibleEquipmentFactory
    {
        private readonly ILogger<ExtensibleEquipmentFactory> _logger;
        private readonly IEquipmentTypeRegistry _typeRegistry;

        public ExtensibleEquipmentFactory(
            ILogger<ExtensibleEquipmentFactory> logger,
            IEquipmentTypeRegistry typeRegistry)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
        }

        public BaseEquipmentData CreateEquipment(string typeName, Dictionary<string, object>? parameters = null)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));

            _logger.LogDebug($"Creating equipment of type {typeName} with {parameters?.Count ?? 0} parameters");

            var equipment = _typeRegistry.CreateEquipment(typeName);
            
            if (parameters != null && parameters.Any())
            {
                ApplyParameters(equipment, parameters);
            }

            ApplyDefaults(equipment);
            
            _logger.LogInformation($"Created {typeName} equipment: {equipment.PC_Name ?? "Unnamed"} ({equipment.Serial_No ?? "No Serial"})");
            return equipment;
        }

        public T CreateEquipment<T>(Dictionary<string, object>? parameters = null) where T : BaseEquipmentData, new()
        {
            _logger.LogDebug($"Creating equipment of type {typeof(T).Name} with {parameters?.Count ?? 0} parameters");

            var equipment = new T();
            
            if (parameters != null && parameters.Any())
            {
                ApplyParameters(equipment, parameters);
            }

            ApplyDefaults(equipment);
            
            _logger.LogInformation($"Created {typeof(T).Name} equipment: {equipment.PC_Name ?? "Unnamed"} ({equipment.Serial_No ?? "No Serial"})");
            return equipment;
        }

        public bool SupportsType(string typeName)
        {
            return _typeRegistry.IsRegistered(typeName);
        }

        public IEnumerable<string> GetSupportedTypes()
        {
            return _typeRegistry.GetRegisteredTypes();
        }

        private void ApplyParameters(BaseEquipmentData equipment, Dictionary<string, object> parameters)
        {
            foreach (var kvp in parameters)
            {
                try
                {
                    ApplyParameter(equipment, kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to apply parameter {kvp.Key} with value {kvp.Value}");
                }
            }
        }

        private void ApplyParameter(BaseEquipmentData equipment, string parameterName, object value)
        {
            if (equipment == null || string.IsNullOrWhiteSpace(parameterName) || value == null)
                return;

            var stringValue = value.ToString();

            switch (parameterName.ToUpperInvariant())
            {
                case "PCNAME":
                case "PC_NAME":
                case "NAME":
                    equipment.PC_Name = stringValue;
                    break;

                case "SERIALNO":
                case "SERIAL_NO":
                case "SERIAL":
                    equipment.Serial_No = stringValue;
                    break;

                case "APPOWNER":
                case "APP_OWNER":
                case "OWNER":
                    equipment.App_Owner = stringValue;
                    break;

                case "STATUS":
                    equipment.Status = stringValue;
                    break;

                case "TYPE":
                    equipment.MachineType = stringValue;
                    break;

                case "LOCATION":
                    equipment.Note = stringValue;
                    break;

                case "CREATORINITIALS":
                case "CREATOR_INITIALS":
                case "CREATOR":
                    equipment.Creator_Initials = stringValue;
                    break;

                case "ENTRYDATE":
                case "ENTRY_DATE":
                case "DATE":
                    if (value is DateTime dateValue)
                    {
                        equipment.Entry_Date = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else if (DateTime.TryParse(stringValue, out var parsedDate))
                    {
                        equipment.Entry_Date = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    break;

                default:
                    _logger.LogDebug($"Unknown parameter: {parameterName} = {stringValue}");
                    break;
            }
        }

        private void ApplyDefaults(BaseEquipmentData equipment)
        {
            if (equipment == null) return;

            // Apply default values for missing required fields
            if (string.IsNullOrWhiteSpace(equipment.Entry_Date))
            {
                equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (string.IsNullOrWhiteSpace(equipment.Status))
            {
                equipment.Status = "Active";
            }

            if (string.IsNullOrWhiteSpace(equipment.Creator_Initials))
            {
                equipment.Creator_Initials = "System";
            }

            // Generate default serial number if not provided
            if (string.IsNullOrWhiteSpace(equipment.Serial_No))
            {
                equipment.Serial_No = $"AUTO-{DateTime.Now:yyyyMMddHHmmss}";
                _logger.LogDebug($"Generated automatic serial number: {equipment.Serial_No}");
            }

            // Generate default PC name if not provided
            if (string.IsNullOrWhiteSpace(equipment.PC_Name))
            {
                var typePrefix = string.IsNullOrWhiteSpace(equipment.MachineType) ? "EQ" : equipment.MachineType.Substring(0, Math.Min(3, equipment.MachineType.Length)).ToUpper();
                equipment.PC_Name = $"{typePrefix}-{DateTime.Now:MMddHHmm}";
                _logger.LogDebug($"Generated automatic PC name: {equipment.PC_Name}");
            }
        }
    }
}