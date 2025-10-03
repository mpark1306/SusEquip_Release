using SusEquip.Data.Models;
using SusEquip.Data.Events;

namespace SusEquip.Data.Services.Composition;

/// <summary>
/// Interface for orchestration services that coordinate complex business processes
/// Implements saga pattern and multi-service coordination for equipment management
/// </summary>
public interface IEquipmentOrchestrationService
{
    /// <summary>
    /// Orchestrates a complete equipment procurement process from request to deployment
    /// </summary>
    /// <param name="request">Procurement orchestration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Orchestration result with process state</returns>
    Task<OrchestrationResult> OrchestrateProcurementProcessAsync(
        ProcurementOrchestrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Orchestrates equipment maintenance workflow with scheduling and resource allocation
    /// </summary>
    /// <param name="request">Maintenance orchestration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Orchestration result for maintenance workflow</returns>
    Task<OrchestrationResult> OrchestrateMaintenanceWorkflowAsync(
        MaintenanceOrchestrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Orchestrates equipment retirement process with data archival and disposal
    /// </summary>
    /// <param name="request">Retirement orchestration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Orchestration result for retirement process</returns>
    Task<OrchestrationResult> OrchestrateRetirementProcessAsync(
        RetirementOrchestrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Orchestrates equipment migration between locations or systems
    /// </summary>
    /// <param name="request">Migration orchestration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Orchestration result for migration process</returns>
    Task<OrchestrationResult> OrchestrateMigrationProcessAsync(
        MigrationOrchestrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of an orchestration process
    /// </summary>
    /// <param name="orchestrationId">Orchestration process ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current orchestration state</returns>
    Task<OrchestrationState> GetOrchestrationStateAsync(
        Guid orchestrationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused or failed orchestration process
    /// </summary>
    /// <param name="orchestrationId">Orchestration process ID</param>
    /// <param name="resumeOptions">Options for resuming the process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Resume operation result</returns>
    Task<OrchestrationResult> ResumeOrchestrationAsync(
        Guid orchestrationId,
        OrchestrationResumeOptions resumeOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an active orchestration process with rollback options
    /// </summary>
    /// <param name="orchestrationId">Orchestration process ID</param>
    /// <param name="rollbackCompletedSteps">Whether to rollback completed steps</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation operation result</returns>
    Task<OrchestrationCancellationResult> CancelOrchestrationAsync(
        Guid orchestrationId,
        bool rollbackCompletedSteps = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Base request model for orchestration operations
/// </summary>
public abstract class OrchestrationRequestBase
{
    public Guid OrchestrationId { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public string? RequestReason { get; set; }
    public OrchestrationOptions Options { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Request model for procurement orchestration
/// </summary>
public class ProcurementOrchestrationRequest : OrchestrationRequestBase
{
    public required IEnumerable<ProcurementItem> Items { get; set; }
    public required string Vendor { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? RequiredDeliveryDate { get; set; }
    public string? DeliveryLocation { get; set; }
    public ProcurementPriority Priority { get; set; } = ProcurementPriority.Normal;
}

/// <summary>
/// Procurement item details
/// </summary>
public class ProcurementItem
{
    public required string ItemType { get; set; }
    public required string ModelNumber { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
}

/// <summary>
/// Procurement priority levels
/// </summary>
public enum ProcurementPriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// Request model for maintenance orchestration
/// </summary>
public class MaintenanceOrchestrationRequest : OrchestrationRequestBase
{
    public required IEnumerable<int> EquipmentIds { get; set; }
    public required MaintenanceType MaintenanceType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public IEnumerable<string> RequiredResources { get; set; } = new List<string>();
    public string? MaintenanceInstructions { get; set; }
}

/// <summary>
/// Maintenance types
/// </summary>
public enum MaintenanceType
{
    Preventive,
    Corrective,
    Emergency,
    Upgrade
}

/// <summary>
/// Request model for retirement orchestration
/// </summary>
public class RetirementOrchestrationRequest : OrchestrationRequestBase
{
    public required IEnumerable<int> EquipmentIds { get; set; }
    public required RetirementReason Reason { get; set; }
    public DateTime? RetirementDate { get; set; }
    public bool ArchiveData { get; set; } = true;
    public string? DisposalMethod { get; set; }
    public string? ReplacementPlan { get; set; }
}

/// <summary>
/// Retirement reasons
/// </summary>
public enum RetirementReason
{
    EndOfLife,
    Obsolete,
    Damaged,
    Replaced,
    Surplus
}

/// <summary>
/// Request model for migration orchestration
/// </summary>
public class MigrationOrchestrationRequest : OrchestrationRequestBase
{
    public required IEnumerable<int> EquipmentIds { get; set; }
    public required string SourceLocation { get; set; }
    public required string TargetLocation { get; set; }
    public DateTime? MigrationDate { get; set; }
    public bool TransferOwnership { get; set; } = false;
    public string? MigrationNotes { get; set; }
}

/// <summary>
/// Orchestration configuration options
/// </summary>
public class OrchestrationOptions
{
    public bool FailOnFirstError { get; set; } = false;
    public bool EnableRollback { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
    public bool SendNotifications { get; set; } = true;
    public bool GenerateAuditLog { get; set; } = true;
}

/// <summary>
/// Result model for orchestration operations
/// </summary>
public class OrchestrationResult
{
    public bool Success { get; set; }
    public Guid OrchestrationId { get; set; }
    public OrchestrationStatus Status { get; set; }
    public List<OrchestrationStep> CompletedSteps { get; set; } = new();
    public List<OrchestrationStep> FailedSteps { get; set; } = new();
    public List<OrchestrationStep> PendingSteps { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public TimeSpan TotalProcessingTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Dictionary<string, object> ResultContext { get; set; } = new();
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}

/// <summary>
/// Orchestration status values
/// </summary>
public enum OrchestrationStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Paused,
    RolledBack
}

/// <summary>
/// Individual orchestration step
/// </summary>
public class OrchestrationStep
{
    public required string StepId { get; set; }
    public required string StepName { get; set; }
    public OrchestrationStepStatus Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration => EndTime - StartTime;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> StepContext { get; set; } = new();
    public int RetryCount { get; set; }
}

/// <summary>
/// Orchestration step status values
/// </summary>
public enum OrchestrationStepStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped,
    RolledBack
}

/// <summary>
/// Current state of an orchestration process
/// </summary>
public class OrchestrationState
{
    public Guid OrchestrationId { get; set; }
    public OrchestrationStatus Status { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int FailedSteps { get; set; }
    public double ProgressPercentage => TotalSteps > 0 ? (double)CompletedSteps / TotalSteps * 100 : 0;
    public string? CurrentStepName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? LastUpdated { get; set; }
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    public Dictionary<string, object> StateContext { get; set; } = new();
}

/// <summary>
/// Options for resuming orchestration
/// </summary>
public class OrchestrationResumeOptions
{
    public bool RetryFailedSteps { get; set; } = true;
    public bool SkipFailedSteps { get; set; } = false;
    public OrchestrationOptions? UpdatedOptions { get; set; }
    public Dictionary<string, object> AdditionalContext { get; set; } = new();
}

/// <summary>
/// Result model for orchestration cancellation
/// </summary>
public class OrchestrationCancellationResult
{
    public bool Success { get; set; }
    public Guid OrchestrationId { get; set; }
    public bool RollbackPerformed { get; set; }
    public List<string> RollbackErrors { get; set; } = new();
    public DateTime CancellationTime { get; set; }
    public List<IDomainEvent> PublishedEvents { get; set; } = new();
}