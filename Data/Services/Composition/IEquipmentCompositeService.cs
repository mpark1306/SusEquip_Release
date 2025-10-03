using SusEquip.Data.Models;
using SusEquip.Data.Events;

namespace SusEquip.Data.Services.Composition;

/// <summary>
/// Interface for composite equipment services that handle complex multi-step operations
/// Coordinates multiple services to perform comprehensive equipment management tasks
/// </summary>
public interface IEquipmentCompositeService
{
    /// <summary>
    /// Performs a complete equipment lifecycle operation from creation to deployment
    /// </summary>
    /// <param name="request">Equipment lifecycle request with all necessary data</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Result containing the processed equipment and operation summary</returns>
    Task<EquipmentLifecycleResult> ProcessEquipmentLifecycleAsync(
        EquipmentLifecycleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and prepares equipment for deployment with all prerequisite checks
    /// </summary>
    /// <param name="equipmentIds">Collection of equipment IDs to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with detailed feedback</returns>
    Task<EquipmentDeploymentValidationResult> ValidateForDeploymentAsync(
        IEnumerable<int> equipmentIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs equipment status transition with validation and event publishing
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="targetStatus">Target status to transition to</param>
    /// <param name="reason">Reason for status change</param>
    /// <param name="userId">User performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status transition result</returns>
    Task<EquipmentStatusTransitionResult> TransitionEquipmentStatusAsync(
        int equipmentId,
        string targetStatus,
        string reason,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives equipment with related data cleanup and audit trail
    /// </summary>
    /// <param name="equipmentIds">Equipment IDs to archive</param>
    /// <param name="archiveReason">Reason for archiving</param>
    /// <param name="userId">User performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Archive operation result</returns>
    Task<EquipmentArchiveResult> ArchiveEquipmentAsync(
        IEnumerable<int> equipmentIds,
        string archiveReason,
        string userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for equipment lifecycle operations
/// </summary>
public class EquipmentLifecycleRequest
{
    public required string SerialNumber { get; set; }
    public required string ModelNumber { get; set; }
    public required string MachineType { get; set; }
    public required string InitialStatus { get; set; }
    public required string AssignedLocation { get; set; }
    public required string UserId { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; } = new();
    public bool SkipValidation { get; set; } = false;
    public bool AutoDeploy { get; set; } = false;
}

/// <summary>
/// Result model for equipment lifecycle operations
/// </summary>
public class EquipmentLifecycleResult
{
    public bool Success { get; set; }
    public EquipmentData? Equipment { get; set; }
    public List<string> ProcessedSteps { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingDuration { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}

/// <summary>
/// Result model for deployment validation
/// </summary>
public class EquipmentDeploymentValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<int, List<string>> ValidationIssues { get; set; } = new();
    public List<int> ReadyForDeployment { get; set; } = new();
    public List<int> RequiresAttention { get; set; } = new();
    public Dictionary<string, int> ValidationSummary { get; set; } = new();
}

/// <summary>
/// Result model for status transitions
/// </summary>
public class EquipmentStatusTransitionResult
{
    public bool Success { get; set; }
    public string? PreviousStatus { get; set; }
    public string? CurrentStatus { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public DateTime TransitionTimestamp { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}

/// <summary>
/// Result model for archive operations
/// </summary>
public class EquipmentArchiveResult
{
    public bool Success { get; set; }
    public int ArchivedCount { get; set; }
    public int FailedCount { get; set; }
    public Dictionary<int, string> ArchiveDetails { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}