using SusEquip.Data.Models;

namespace SusEquip.Data.Services.Validation
{
    /// <summary>
    /// Strategy for validating date-related fields in equipment data
    /// </summary>
    public class DateValidationStrategy : IValidationStrategy
    {
        public string StrategyName => "Date Validation";
        
        public int Priority => 1;

        public bool CanValidate(BaseEquipmentData equipment)
        {
            return equipment != null;
        }

        public Task<IEnumerable<ValidationIssue>> ValidateAsync(BaseEquipmentData equipmentData)
        {
            var issues = new List<ValidationIssue>();

            // Validate service start date
            var serviceStartIssue = ValidateServiceStartDate(equipmentData);
            if (serviceStartIssue != null)
            {
                issues.Add(serviceStartIssue);
            }

            // Validate service end date
            var serviceEndIssue = ValidateServiceEndDate(equipmentData);
            if (serviceEndIssue != null)
            {
                issues.Add(serviceEndIssue);
            }

            // Validate entry date
            var entryDateIssue = ValidateEntryDate(equipmentData);
            if (entryDateIssue != null)
            {
                issues.Add(entryDateIssue);
            }

            return Task.FromResult<IEnumerable<ValidationIssue>>(issues);
        }

        /// <summary>
        /// Validates service start date for equipment entries
        /// </summary>
        private ValidationIssue? ValidateServiceStartDate(BaseEquipmentData equipmentData)
        {
            if (string.IsNullOrEmpty(equipmentData.Service_Start))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Service_Start),
                    "", "", "Service start date is required", "Medium");
            }

            if (!DateTime.TryParse(equipmentData.Service_Start, out DateTime serviceStartDate))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Service_Start),
                    equipmentData.Service_Start, "", $"Invalid service start date format: {equipmentData.Service_Start}", "High");
            }

            // Validate service start date is not too far in the future
            if (serviceStartDate > DateTime.Now.AddDays(30)) // Allow 30 days buffer for scheduled deployments
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Service_Start),
                    equipmentData.Service_Start, DateTime.Now.ToString("yyyy-MM-dd"), "Service start date is too far in the future", "Medium");
            }

            // Validate service start date is not too old (before 1990)
            if (serviceStartDate < new DateTime(1990, 1, 1))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Service_Start),
                    equipmentData.Service_Start, "", "Service start date is too old", "Medium");
            }

            return null;
        }

        /// <summary>
        /// Validates service end date for equipment entries
        /// </summary>
        private ValidationIssue? ValidateServiceEndDate(BaseEquipmentData equipmentData)
        {
            if (string.IsNullOrEmpty(equipmentData.Service_Ends))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Service_Ends),
                    "", "", "Service end date is required", "Medium");
            }

            if (!DateTime.TryParse(equipmentData.Service_Ends, out DateTime serviceEndDate))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Service_Ends),
                    equipmentData.Service_Ends, "", $"Invalid service end date format: {equipmentData.Service_Ends}", "High");
            }

            // Check if service end date is before service start date
            if (!string.IsNullOrEmpty(equipmentData.Service_Start) && 
                DateTime.TryParse(equipmentData.Service_Start, out DateTime serviceStartDate))
            {
                if (serviceEndDate < serviceStartDate)
                {
                    return CreateIssue(equipmentData, nameof(equipmentData.Service_Ends),
                        equipmentData.Service_Ends, equipmentData.Service_Start, "Service end date cannot be before service start date", "High");
                }
            }

            return null;
        }

        /// <summary>
        /// Validates entry date for equipment entries
        /// </summary>
        private ValidationIssue? ValidateEntryDate(BaseEquipmentData equipmentData)
        {
            if (string.IsNullOrEmpty(equipmentData.Entry_Date))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Entry_Date),
                    "", "", "Entry date is required", "High");
            }

            if (!DateTime.TryParse(equipmentData.Entry_Date, out DateTime entryDate))
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Entry_Date),
                    equipmentData.Entry_Date, "", $"Invalid entry date format: {equipmentData.Entry_Date}", "High");
            }

            // Validate entry date is not too far in the future
            if (entryDate > DateTime.Now.AddDays(1)) // Allow 1 day buffer for time zones
            {
                return CreateIssue(equipmentData, nameof(equipmentData.Entry_Date),
                    equipmentData.Entry_Date, DateTime.Now.ToString("yyyy-MM-dd"), "Entry date cannot be in the future", "High");
            }

            return null;
        }

        /// <summary>
        /// Creates a validation issue with consistent formatting
        /// </summary>
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
                CurrentValue = currentValue,
                SuggestedValue = expectedValue,
                Reason = description,
                Severity = severity,
                IssueType = "Date",
                DetectedDate = DateTime.Now,
                EquipmentData = equipmentData as EquipmentData
            };
        }
    }
}