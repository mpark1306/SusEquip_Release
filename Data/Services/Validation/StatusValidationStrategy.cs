using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;

namespace SusEquip.Data.Services.Validation
{
    /// <summary>
    /// Strategy for validating status-related fields in equipment data
    /// </summary>
    public class StatusValidationStrategy : IValidationStrategy
    {
        private readonly ILogger<StatusValidationStrategy> Logger;

        public string StrategyName => "Status Validation";
        
        public int Priority => 2;

        public bool CanValidate(BaseEquipmentData equipment)
        {
            return equipment != null;
        }

        private readonly List<string> _validStatuses = new()
        {
            "Active",
            "Inactive", 
            "Maintenance",
            "Damaged",
            "Lost",
            "Disposed",
            "Retired",
            "Pending",
            "Reserved",
            "Kasseret" // Danish for "Discarded"
        };

        public StatusValidationStrategy(ILogger<StatusValidationStrategy> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<IEnumerable<ValidationIssue>> ValidateAsync(BaseEquipmentData equipmentData)
        {
            var issues = new List<ValidationIssue>();

            try
            {
                // Check if status is valid
                if (!IsValidStatus(equipmentData.Status))
                {
                    var suggestedStatus = GetClosestValidStatus(equipmentData.Status);
                    issues.Add(CreateIssue(equipmentData, nameof(equipmentData.Status),
                        equipmentData.Status, suggestedStatus,
                        $"Status '{equipmentData.Status}' is not a valid status", "Medium"));
                }

                // Check status consistency with other fields
                issues.AddRange(ValidateStatusConsistency(equipmentData));

                // Check for suspicious status patterns
                issues.AddRange(ValidateStatusPatterns(equipmentData));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating status for equipment {PCName}", equipmentData.PC_Name);
            }

            return Task.FromResult<IEnumerable<ValidationIssue>>(issues);
        }

        private bool IsValidStatus(string status)
        {
            return _validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        private string GetClosestValidStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Pending";

            // Try exact match (case insensitive)
            var exactMatch = _validStatuses.FirstOrDefault(s => 
                string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
                return exactMatch;

            // Try partial match
            var partialMatch = _validStatuses.FirstOrDefault(s => 
                s.ToLower().Contains(status.ToLower()) || status.ToLower().Contains(s.ToLower()));
            if (partialMatch != null)
                return partialMatch;

            // Common mappings
            var commonMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["In Use"] = "Active",
                ["In-Use"] = "Active",
                ["Working"] = "Active",
                ["OK"] = "Active",
                ["Good"] = "Active",
                ["Available"] = "Active",
                ["Not In Use"] = "Inactive",
                ["Not Used"] = "Inactive",
                ["Unused"] = "Inactive",
                ["Offline"] = "Inactive",
                ["Down"] = "Maintenance",
                ["Broken"] = "Damaged",
                ["Defect"] = "Damaged",
                ["Faulty"] = "Damaged",
                ["Missing"] = "Lost",
                ["Gone"] = "Lost",
                ["Discarded"] = "Disposed",
                ["Thrown Away"] = "Disposed",
                ["Scrapped"] = "Disposed",
                ["End of Life"] = "Retired",
                ["EOL"] = "Retired",
                ["Old"] = "Retired"
            };

            return commonMappings.GetValueOrDefault(status, "Pending");
        }

        private IEnumerable<ValidationIssue> ValidateStatusConsistency(BaseEquipmentData equipment)
        {
            var issues = new List<ValidationIssue>();

            // Check if equipment is marked as active but service period has ended
            if (string.Equals(equipment.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(equipment.Service_Ends) && 
                    DateTime.TryParse(equipment.Service_Ends, out DateTime serviceEndDate))
                {
                    if (serviceEndDate < DateTime.Now)
                    {
                        issues.Add(CreateIssue(equipment, nameof(equipment.Status),
                            equipment.Status, "Inactive",
                            "Equipment marked as active but service period has ended", "Medium"));
                    }
                }
            }

            // Check if equipment is marked as disposed/retired but still has recent activity
            if (IsTerminalStatus(equipment.Status))
            {
                // Check if entry date is very recent for disposed equipment
                if (!string.IsNullOrEmpty(equipment.Entry_Date) && 
                    DateTime.TryParse(equipment.Entry_Date, out DateTime entryDate))
                {
                    var daysSinceEntry = (DateTime.Now - entryDate).TotalDays;
                    if (daysSinceEntry < 30) // Less than 30 days old but marked as disposed
                    {
                        issues.Add(CreateIssue(equipment, nameof(equipment.Status),
                            equipment.Status, "Pending",
                            "Recently entered equipment marked as disposed - verify status", "Low"));
                    }
                }
            }

            return issues;
        }

        private IEnumerable<ValidationIssue> ValidateStatusPatterns(BaseEquipmentData equipment)
        {
            var issues = new List<ValidationIssue>();

            // Check for inconsistent status patterns
            var pcName = equipment.PC_Name?.ToLower() ?? "";
            var status = equipment.Status?.ToLower() ?? "";

            // Check if PC name suggests it's a test/demo machine but status indicates production use
            if ((pcName.Contains("test") || pcName.Contains("demo") || pcName.Contains("temp")) &&
                string.Equals(equipment.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(CreateIssue(equipment, nameof(equipment.Status),
                    equipment.Status, "Reserved",
                    "Test/Demo machine marked as active - consider 'Reserved' status", "Low"));
            }

            // Check for suspicious status combinations with department
            if (!string.IsNullOrEmpty(equipment.Department))
            {
                var dept = equipment.Department.ToLower();
                if (dept.Contains("storage") && 
                    string.Equals(equipment.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(CreateIssue(equipment, nameof(equipment.Status),
                        equipment.Status, "Inactive",
                        "Equipment in storage department marked as active", "Medium"));
                }
            }

            return issues;
        }

        private bool IsTerminalStatus(string status)
        {
            var terminalStatuses = new[] { "Disposed", "Retired", "Lost", "Kasseret" };
            return terminalStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        private ValidationIssue CreateIssue(BaseEquipmentData equipmentData, string fieldName, 
            string currentValue, string expectedValue, string description, string severity)
        {
            // Get InstNo safely - cast to EquipmentData if needed
            var instNo = 0;
            if (equipmentData is EquipmentData ed)
                instNo = ed.Inst_No;
            
            return new ValidationIssue
            {
                EntryId = equipmentData.EntryId,
                InstNo = instNo,
                PCName = equipmentData.PC_Name,
                FieldName = fieldName,
                CurrentValue = currentValue ?? string.Empty,
                SuggestedValue = expectedValue,
                Reason = description,
                Severity = severity,
                IssueType = "Status",
                DetectedDate = DateTime.Now,
                EquipmentData = equipmentData as EquipmentData
            };
        }
    }
}