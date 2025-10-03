using SusEquip.Data.Models;

namespace SusEquip.Data.Models
{
    public class ValidationIssue
    {
        public int EntryId { get; set; }
        public int InstNo { get; set; }
        public string PCName { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string SuggestedValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
        public DateTime DetectedDate { get; set; } = DateTime.Now;
        public EquipmentData? EquipmentData { get; set; }
    }

    public class DataCorrection
    {
        public int CorrectionId { get; set; }
        public int EntryId { get; set; }
        public int InstNo { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string CorrectorInitials { get; set; } = string.Empty;
        public string AppOwner { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CorrectionDate { get; set; } = DateTime.Now;
        public string IssueType { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
    }

    public class IgnoredIssue
    {
        public int IgnoredIssueId { get; set; }
        public int EntryId { get; set; }
        public int InstNo { get; set; }
        public string PCName { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string SuggestedValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string IgnoredBy { get; set; } = string.Empty;
        public string IgnoreReason { get; set; } = string.Empty;
        public DateTime IgnoredDate { get; set; } = DateTime.Now;
    }

    public class SolvedIssue
    {
        public int SolvedIssueId { get; set; }
        public int EntryId { get; set; }
        public int InstNo { get; set; }
        public string PCName { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string SuggestedValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string SolvedBy { get; set; } = string.Empty;
        public string SolutionMethod { get; set; } = string.Empty; // "Manual Fix", "Auto Correction", "Status Change", etc.
        public string SolutionNotes { get; set; } = string.Empty;
        public DateTime SolvedDate { get; set; } = DateTime.Now;
    }
}
