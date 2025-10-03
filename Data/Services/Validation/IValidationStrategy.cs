using SusEquip.Data.Models;

namespace SusEquip.Data.Services.Validation
{
    /// <summary>
    /// Interface for validation strategies that can be applied to equipment
    /// </summary>
    public interface IValidationStrategy
    {
        /// <summary>
        /// Validates equipment and returns any issues found
        /// </summary>
        Task<IEnumerable<ValidationIssue>> ValidateAsync(BaseEquipmentData equipment);
        
        /// <summary>
        /// Determines if this strategy can validate the given equipment
        /// </summary>
        bool CanValidate(BaseEquipmentData equipment);
        
        /// <summary>
        /// Name of this validation strategy
        /// </summary>
        string StrategyName { get; }
        
        /// <summary>
        /// Priority of this strategy (higher priority runs first)
        /// </summary>
        int Priority { get; }
    }

    /// <summary>
    /// Base class for validation strategies providing common functionality
    /// </summary>
    public abstract class BaseValidationStrategy : IValidationStrategy
    {
        protected readonly ILogger Logger;

        protected BaseValidationStrategy(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task<IEnumerable<ValidationIssue>> ValidateAsync(BaseEquipmentData equipment);
        public abstract bool CanValidate(BaseEquipmentData equipment);
        public abstract string StrategyName { get; }
        public virtual int Priority => 0;

        /// <summary>
        /// Helper method to create validation issues
        /// </summary>
        protected ValidationIssue CreateIssue(BaseEquipmentData equipment, string fieldName, 
            string currentValue, string suggestedValue, string reason, string severity = "Medium")
        {
            return new ValidationIssue
            {
                EntryId = equipment.EntryId,
                InstNo = equipment is EquipmentData eq ? eq.Inst_No : 0,
                PCName = equipment.PC_Name,
                IssueType = StrategyName,
                FieldName = fieldName,
                CurrentValue = currentValue,
                SuggestedValue = suggestedValue,
                Reason = reason,
                Severity = severity,
                DetectedDate = DateTime.Now
            };
        }
    }
}