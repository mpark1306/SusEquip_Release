using SusEquip.Data.Models;
using SusEquip.Data.Events;

namespace SusEquip.Data.Services.Composition;

/// <summary>
/// Interface for batch processing operations on equipment collections
/// Provides bulk operations with progress tracking, error aggregation, and rollback capabilities
/// </summary>
public interface IBatchProcessingService
{
    /// <summary>
    /// Performs bulk import of equipment from external data sources
    /// </summary>
    /// <param name="request">Batch import request with data and options</param>
    /// <param name="progressCallback">Optional progress callback for UI updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch import result with detailed statistics</returns>
    Task<BatchImportResult> BulkImportEquipmentAsync(
        BatchImportRequest request,
        IProgress<BatchProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs bulk update operations on equipment collections
    /// </summary>
    /// <param name="request">Batch update request</param>
    /// <param name="progressCallback">Optional progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch update result</returns>
    Task<BatchUpdateResult> BulkUpdateEquipmentAsync(
        BatchUpdateRequest request,
        IProgress<BatchProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs bulk status changes with validation and rollback support
    /// </summary>
    /// <param name="equipmentIds">Equipment IDs to update</param>
    /// <param name="targetStatus">Target status for all equipment</param>
    /// <param name="reason">Reason for the batch status change</param>
    /// <param name="userId">User performing the operation</param>
    /// <param name="rollbackOnFailure">Whether to rollback all changes if any fail</param>
    /// <param name="progressCallback">Optional progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch status change result</returns>
    Task<BatchStatusChangeResult> BulkChangeStatusAsync(
        IEnumerable<int> equipmentIds,
        string targetStatus,
        string reason,
        string userId,
        bool rollbackOnFailure = true,
        IProgress<BatchProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a large collection of equipment in batches
    /// </summary>
    /// <param name="equipmentIds">Equipment IDs to validate</param>
    /// <param name="validationRules">Validation rules to apply</param>
    /// <param name="batchSize">Size of each validation batch</param>
    /// <param name="progressCallback">Optional progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch validation result</returns>
    Task<BatchValidationResult> BulkValidateEquipmentAsync(
        IEnumerable<int> equipmentIds,
        IEnumerable<string> validationRules,
        int batchSize = 100,
        IProgress<BatchProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs bulk deletion with safety checks and audit trail
    /// </summary>
    /// <param name="equipmentIds">Equipment IDs to delete</param>
    /// <param name="reason">Reason for bulk deletion</param>
    /// <param name="userId">User performing the operation</param>
    /// <param name="safetyChecks">Whether to perform safety checks before deletion</param>
    /// <param name="progressCallback">Optional progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch deletion result</returns>
    Task<BatchDeleteResult> BulkDeleteEquipmentAsync(
        IEnumerable<int> equipmentIds,
        string reason,
        string userId,
        bool safetyChecks = true,
        IProgress<BatchProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Progress information for batch operations
/// </summary>
public class BatchProgress
{
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public double PercentComplete => TotalItems > 0 ? (double)ProcessedItems / TotalItems * 100 : 0;
    public string? CurrentOperation { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}

/// <summary>
/// Request model for batch import operations
/// </summary>
public class BatchImportRequest
{
    public required IEnumerable<EquipmentImportData> EquipmentData { get; set; }
    public bool ValidateBeforeImport { get; set; } = true;
    public bool SkipDuplicates { get; set; } = true;
    public bool RollbackOnError { get; set; } = false;
    public int BatchSize { get; set; } = 50;
    public string? ImportSource { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string> ImportOptions { get; set; } = new();
}

/// <summary>
/// Data model for equipment import
/// </summary>
public class EquipmentImportData
{
    public required string SerialNumber { get; set; }
    public string? ModelNumber { get; set; }
    public string? MachineType { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public Dictionary<string, object> AdditionalFields { get; set; } = new();
}

/// <summary>
/// Result model for batch import operations
/// </summary>
public class BatchImportResult
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public int SkippedDuplicates { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, int> ImportStatistics { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}

/// <summary>
/// Request model for batch update operations
/// </summary>
public class BatchUpdateRequest
{
    public required IEnumerable<EquipmentUpdateData> Updates { get; set; }
    public bool ValidateUpdates { get; set; } = true;
    public bool RollbackOnError { get; set; } = false;
    public int BatchSize { get; set; } = 50;
    public string? UserId { get; set; }
}

/// <summary>
/// Data model for equipment updates
/// </summary>
public class EquipmentUpdateData
{
    public required int EquipmentId { get; set; }
    public Dictionary<string, object> FieldUpdates { get; set; } = new();
    public string? UpdateReason { get; set; }
}

/// <summary>
/// Result model for batch update operations
/// </summary>
public class BatchUpdateResult
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessfulUpdates { get; set; }
    public int FailedUpdates { get; set; }
    public Dictionary<int, List<string>> UpdateErrors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}

/// <summary>
/// Result model for batch status change operations
/// </summary>
public class BatchStatusChangeResult
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessfulChanges { get; set; }
    public int FailedChanges { get; set; }
    public bool RolledBack { get; set; }
    public Dictionary<int, string> ChangeDetails { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}

/// <summary>
/// Result model for batch validation operations
/// </summary>
public class BatchValidationResult
{
    public bool AllValid { get; set; }
    public int TotalValidated { get; set; }
    public int ValidItems { get; set; }
    public int InvalidItems { get; set; }
    public Dictionary<int, List<string>> ValidationErrors { get; set; } = new();
    public Dictionary<string, int> ValidationSummary { get; set; } = new();
    public TimeSpan ValidationTime { get; set; }
}

/// <summary>
/// Result model for batch delete operations
/// </summary>
public class BatchDeleteResult
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessfulDeletes { get; set; }
    public int FailedDeletes { get; set; }
    public int SafetyCheckFailures { get; set; }
    public Dictionary<int, string> DeletionDetails { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}