using SusEquip.Data.Models;

namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for validation service operations
    /// </summary>
    public interface IValidationService
    {
        Task<List<ValidationIssue>> ValidateAllEquipmentAsync();
        Task<List<ValidationIssue>> ValidateEquipmentAsync(EquipmentData equipment);
        Task<bool> IsValidEquipmentAsync(EquipmentData equipment);
        
        // Issue management
        Task IgnoreIssueAsync(int issueId, string reason, string ignoredBy);
        Task UnignoreIssueAsync(int issueId);
        Task MarkIssueSolvedAsync(int issueId, string solution, string solvedBy);
        
        // Issue retrieval
        Task<List<int>> GetIgnoredIssueIdsAsync();
        Task<List<int>> GetSolvedIssueIdsAsync();
        
        // Validation rules
        Task<bool> IsValidMacAddressAsync(string macAddress);
        Task<bool> IsValidUuidAsync(string uuid);
        Task<bool> IsValidInstNoAsync(int instNo);
        Task<bool> IsValidSerialNoAsync(string serialNo);
    }
}