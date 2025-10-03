using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using System.Text.RegularExpressions;

namespace SusEquip.Data.Services.Validation
{
    /// <summary>
    /// Validation strategy for PC names
    /// </summary>
    public class PCNameValidationStrategy : BaseValidationStrategy
    {
        public PCNameValidationStrategy(ILogger<PCNameValidationStrategy> logger) : base(logger) { }

        public override string StrategyName => "PCNameValidation";
        public override int Priority => 75;

        public override bool CanValidate(BaseEquipmentData equipment)
        {
            return !string.IsNullOrWhiteSpace(equipment.PC_Name);
        }

        public override Task<IEnumerable<ValidationIssue>> ValidateAsync(BaseEquipmentData equipment)
        {
            var issues = new List<ValidationIssue>();

            try
            {
                // Check PC name format
                if (!IsValidPCNameFormat(equipment.PC_Name))
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.PC_Name), 
                        equipment.PC_Name, FormatPCName(equipment.PC_Name), 
                        "PC name format doesn't follow naming convention (expected: XX-XXXXXXXX)", "Low"));
                }

                // Check PC name length
                if (equipment.PC_Name.Length > 50)
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.PC_Name), 
                        equipment.PC_Name, equipment.PC_Name.Substring(0, 50), 
                        "PC name is too long (max 50 characters)", "Medium"));
                }

                // Check for forbidden characters
                if (HasForbiddenCharacters(equipment.PC_Name))
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.PC_Name), 
                        equipment.PC_Name, CleanPCName(equipment.PC_Name), 
                        "PC name contains forbidden characters (allowed: A-Z, 0-9, dash, underscore)", "Medium"));
                }

                // Check for placeholder names
                if (IsPlaceholderName(equipment.PC_Name))
                {
                    var suggestedName = equipment is EquipmentData eq ? $"PC-{eq.Inst_No:D6}" : "PC-PENDING";
                    issues.Add(CreateIssue(equipment, nameof(equipment.PC_Name), 
                        equipment.PC_Name, suggestedName, 
                        "PC name appears to be a placeholder", "Low"));
                }

                // Check for department consistency (if department is specified)
                if (!string.IsNullOrWhiteSpace(equipment.Department) && !IsConsistentWithDepartment(equipment.PC_Name, equipment.Department))
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.PC_Name), 
                        equipment.PC_Name, $"{equipment.Department.Substring(0, 2).ToUpper()}-{equipment.PC_Name}", 
                        $"PC name doesn't reflect department '{equipment.Department}'", "Low"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating PC name for equipment {PCName}", equipment.PC_Name);
            }

            return Task.FromResult<IEnumerable<ValidationIssue>>(issues);
        }

        private bool IsValidPCNameFormat(string pcName)
        {
            // Expected format: 2-4 letters followed by dash and 4-8 alphanumeric characters
            return Regex.IsMatch(pcName, @"^[A-Z]{2,4}-[A-Z0-9]{4,8}$", RegexOptions.IgnoreCase);
        }

        private string FormatPCName(string pcName)
        {
            // Clean and format PC name
            var cleaned = pcName.ToUpper().Replace(" ", "-").Replace("_", "-");
            
            // Remove multiple dashes
            cleaned = Regex.Replace(cleaned, "-+", "-");
            
            // Remove leading/trailing dashes
            cleaned = cleaned.Trim('-');
            
            return cleaned;
        }

        private bool HasForbiddenCharacters(string pcName)
        {
            // Only allow A-Z, 0-9, dash, and underscore
            return !Regex.IsMatch(pcName, @"^[A-Z0-9\-_]+$", RegexOptions.IgnoreCase);
        }

        private string CleanPCName(string pcName)
        {
            // Remove forbidden characters
            return Regex.Replace(pcName.ToUpper(), @"[^A-Z0-9\-_]", "");
        }

        private bool IsPlaceholderName(string pcName)
        {
            var placeholderPatterns = new[]
            {
                @"^(TEMP|TEST|DUMMY|PLACEHOLDER)",
                @"^(PC|COMPUTER|MACHINE)\d*$",
                @"^(NEW|OLD|UNKNOWN|PENDING)",
                @"^(TODO|TBD|FIXME)",
                @"^\d+$" // Just numbers
            };

            return placeholderPatterns.Any(pattern => 
                Regex.IsMatch(pcName, pattern, RegexOptions.IgnoreCase));
        }

        private bool IsConsistentWithDepartment(string pcName, string department)
        {
            if (string.IsNullOrWhiteSpace(department) || department.Length < 2)
                return true; // Can't validate without proper department

            var departmentPrefix = department.Substring(0, 2).ToUpper();
            return pcName.ToUpper().StartsWith(departmentPrefix + "-");
        }
    }
}