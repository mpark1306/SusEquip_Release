using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using System.Text.RegularExpressions;

namespace SusEquip.Data.Services.Validation
{
    /// <summary>
    /// Validation strategy for serial numbers
    /// </summary>
    public class SerialNumberValidationStrategy : BaseValidationStrategy
    {
        private readonly IEquipmentService _equipmentService;

        public SerialNumberValidationStrategy(IEquipmentService equipmentService, ILogger<SerialNumberValidationStrategy> logger) 
            : base(logger)
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        }

        public override string StrategyName => "SerialNumberValidation";
        public override int Priority => 100; // High priority

        public override bool CanValidate(BaseEquipmentData equipment)
        {
            return !string.IsNullOrWhiteSpace(equipment.Serial_No);
        }

        public override async Task<IEnumerable<ValidationIssue>> ValidateAsync(BaseEquipmentData equipment)
        {
            var issues = new List<ValidationIssue>();

            try
            {
                // Check if serial number is already taken
                if (equipment is EquipmentData equipData)
                {
                    var isTaken = await _equipmentService.IsSerialNoTakenInMachinesAsync(equipment.Serial_No);
                    if (isTaken)
                    {
                        issues.Add(CreateIssue(equipment, nameof(equipment.Serial_No), 
                            equipment.Serial_No, "Generate new serial number", 
                            $"Serial number already exists in the system", "High"));
                    }
                }

                // Validate serial number format
                if (!IsValidSerialNumberFormat(equipment.Serial_No))
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.Serial_No), 
                        equipment.Serial_No, FormatSerialNumber(equipment.Serial_No), 
                        "Serial number format is invalid. Expected format: 8-12 alphanumeric characters", "Medium"));
                }

                // Check for suspicious patterns
                if (HasSuspiciousPattern(equipment.Serial_No))
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.Serial_No), 
                        equipment.Serial_No, "Review and update serial number", 
                        "Serial number appears to be placeholder or test data", "Low"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating serial number for equipment {PCName}", equipment.PC_Name);
            }

            return issues;
        }

        private bool IsValidSerialNumberFormat(string serialNo)
        {
            // Expected format: 8-12 alphanumeric characters
            return Regex.IsMatch(serialNo, @"^[A-Z0-9]{8,12}$", RegexOptions.IgnoreCase);
        }

        private string FormatSerialNumber(string serialNo)
        {
            // Remove spaces, dashes, and convert to uppercase
            return serialNo.ToUpper().Replace("-", "").Replace(" ", "").Replace("_", "");
        }

        private bool HasSuspiciousPattern(string serialNo)
        {
            var suspiciousPatterns = new[]
            {
                @"^(TEST|TEMP|DUMMY|PLACEHOLDER)",
                @"^(123456|000000|111111|AAAAAA)",
                @"^(N/?A|NONE|NULL|UNKNOWN)",
                @"(TODO|FIXME|TBD)"
            };

            return suspiciousPatterns.Any(pattern => 
                Regex.IsMatch(serialNo, pattern, RegexOptions.IgnoreCase));
        }
    }
}