using SusEquip.Data.Models;

namespace SusEquip.Data.Interfaces.Services
{
    
    /// Interface for data validation service operations
    
    public interface IDataValidationService
    {
        
        /// Detects all validation issues in equipment data
        
        List<ValidationIssue> DetectValidationIssues();
        
        
        /// Validates equipment data before saving
        
        List<ValidationIssue> ValidateEquipmentBeforeSaving(EquipmentData equipment);
        
        
        /// Ignores a validation issue
        
        void IgnoreIssue(ValidationIssue issue, string ignoredBy, string ignoreReason);
        
        
        /// Unignores a previously ignored issue
        
        void UnignoreIssue(int ignoredIssueId);
        
        
        /// Checks if an issue is ignored
        
        bool IsIssueIgnored(int instNo, string fieldName, string issueType);
        
        
        /// Logs a solved issue
        
        void LogSolvedIssue(ValidationIssue issue, string solvedBy, string solutionMethod, string solutionNotes);
        
        
        /// Checks if an issue is solved
        
        bool IsIssueSolved(int instNo, string fieldName, string issueType);
        
        
        /// Gets a summary of validation issues
        
        string GetIssuesSummary(List<ValidationIssue> issues);
        
        
        /// Gets list of ignored issues
        
        List<IgnoredIssue> GetIgnoredIssues();
        
        
        /// Gets list of solved issues
        
        List<SolvedIssue> GetSolvedIssues();
        
        
        /// Gets correction history
        
        List<DataCorrection> GetCorrectionHistory(int? instNo = null);
        
        
        /// Logs a data correction
        
        void LogCorrection(DataCorrection correction);
        
        
        /// Creates missing database tables if needed
        
        void CreateMissingTables();
    }
}