using SusEquip.Data.Models;
using SusEquip.Data.Events;
using SusEquip.Data.Events.Equipment;
using SusEquip.Data.Services;
using SusEquip.Data.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SusEquip.Data.Services.Composition;

/// <summary>
/// Implementation of composite equipment service for complex multi-step operations
/// Coordinates multiple services to perform comprehensive equipment management tasks
/// </summary>
public class EquipmentCompositeService : IEquipmentCompositeService
{
    private readonly IEquipmentService _equipmentService;
    private readonly DataValidationService _validationService;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<EquipmentCompositeService> _logger;

    public EquipmentCompositeService(
        IEquipmentService equipmentService,
        DataValidationService validationService,
        IDomainEventDispatcher eventDispatcher,
        ILogger<EquipmentCompositeService> logger)
    {
        _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EquipmentLifecycleResult> ProcessEquipmentLifecycleAsync(
        EquipmentLifecycleRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new EquipmentLifecycleResult
        {
            Success = false,
            ProcessedSteps = new List<string>(),
            Warnings = new List<string>(),
            Errors = new List<string>(),
            PublishedEvents = new List<IDomainEvent>()
        };

        _logger.LogInformation("Starting equipment lifecycle process for serial number: {SerialNumber}", request.SerialNumber);

        try
        {
            // Step 1: Create equipment data object
            var equipmentData = new EquipmentData
            {
                PC_Name = request.ModelNumber,
                Serial_No = request.SerialNumber,
                MachineType = request.MachineType,
                Status = request.InitialStatus,
                Department = request.AssignedLocation,
                Entry_Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Creator_Initials = request.UserId
            };

            result.ProcessedSteps.Add("Equipment data object created");

            // Step 2: Validate equipment data (if not skipped)
            if (!request.SkipValidation)
            {
                // Basic validation using the equipment's IsValid method
                if (!equipmentData.IsValid())
                {
                    result.Errors.Add("Equipment data validation failed - required fields missing");
                    result.ProcessedSteps.Add("Validation failed");
                    return result;
                }
                result.ProcessedSteps.Add("Validation completed successfully");
            }
            else
            {
                result.Warnings.Add("Validation was skipped as requested");
                result.ProcessedSteps.Add("Validation skipped");
            }

            // Step 3: Get next available Inst_No
            var nextInstNo = await _equipmentService.GetNextInstNoAsync();
            equipmentData.Inst_No = nextInstNo;
            result.ProcessedSteps.Add($"Assigned Inst_No: {nextInstNo}");

            // Step 4: Create equipment in repository
            await _equipmentService.AddEntryAsync(equipmentData);
            result.Equipment = equipmentData;
            result.ProcessedSteps.Add("Equipment created in repository");

            // Step 5: Publish equipment created event
            var createdEvent = new EquipmentCreatedEvent(
                equipmentData,
                request.UserId,
                null,
                "Equipment created through lifecycle process");
            
            await _eventDispatcher.PublishAsync(createdEvent);
            result.PublishedEvents.Add(createdEvent);
            result.ProcessedSteps.Add("Equipment created event published");

            // Step 6: Auto-deploy if requested
            if (request.AutoDeploy)
            {
                var deploymentResult = await TransitionEquipmentStatusAsync(
                    nextInstNo,
                    "Hos Bruger",
                    "Auto-deployment as part of lifecycle process",
                    request.UserId,
                    cancellationToken);

                if (deploymentResult.Success)
                {
                    result.ProcessedSteps.Add("Equipment auto-deployed successfully");
                    result.PublishedEvents.AddRange(deploymentResult.PublishedEvents);
                }
                else
                {
                    result.Warnings.Add("Auto-deployment failed but equipment was created successfully");
                    result.Errors.AddRange(deploymentResult.ValidationErrors);
                    result.ProcessedSteps.Add("Auto-deployment failed");
                }
            }

            result.Success = true;
            _logger.LogInformation("Equipment lifecycle process completed successfully for {SerialNumber}", request.SerialNumber);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error during lifecycle process: {ex.Message}");
            result.ProcessedSteps.Add("Process failed with exception");
            _logger.LogError(ex, "Error during equipment lifecycle process for {SerialNumber}", request.SerialNumber);
        }
        finally
        {
            stopwatch.Stop();
            result.ProcessingDuration = stopwatch.Elapsed;
        }

        return result;
    }

    public Task<EquipmentDeploymentValidationResult> ValidateForDeploymentAsync(
        IEnumerable<int> equipmentIds,
        CancellationToken cancellationToken = default)
    {
        var result = new EquipmentDeploymentValidationResult
        {
            ValidationIssues = new Dictionary<int, List<string>>(),
            ReadyForDeployment = new List<int>(),
            RequiresAttention = new List<int>(),
            ValidationSummary = new Dictionary<string, int>()
        };

        _logger.LogInformation("Starting deployment validation for {Count} equipment items", equipmentIds.Count());

        var totalIssues = 0;
        foreach (var equipmentId in equipmentIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // For simplicity in this demo, we'll use basic validation
                // In a real implementation, you'd retrieve the equipment by ID
                var issues = new List<string>();
                
                // Basic validation checks (placeholder logic)
                if (equipmentId <= 0)
                {
                    issues.Add("Invalid equipment ID");
                }

                if (issues.Any())
                {
                    result.ValidationIssues[equipmentId] = issues;
                    result.RequiresAttention.Add(equipmentId);
                    totalIssues += issues.Count;
                }
                else
                {
                    result.ReadyForDeployment.Add(equipmentId);
                }
            }
            catch (Exception ex)
            {
                result.ValidationIssues[equipmentId] = new List<string> { $"Validation error: {ex.Message}" };
                result.RequiresAttention.Add(equipmentId);
                totalIssues++;
                _logger.LogError(ex, "Error validating equipment {EquipmentId} for deployment", equipmentId);
            }
        }

        // Generate summary
        result.ValidationSummary["TotalEquipment"] = equipmentIds.Count();
        result.ValidationSummary["ReadyForDeployment"] = result.ReadyForDeployment.Count;
        result.ValidationSummary["RequiresAttention"] = result.RequiresAttention.Count;
        result.ValidationSummary["TotalIssues"] = totalIssues;
        
        result.IsValid = !result.RequiresAttention.Any();

        _logger.LogInformation("Deployment validation completed. Ready: {Ready}, Requires attention: {Attention}", 
            result.ReadyForDeployment.Count, result.RequiresAttention.Count);

        return Task.FromResult(result);
    }

    public async Task<EquipmentStatusTransitionResult> TransitionEquipmentStatusAsync(
        int equipmentId,
        string targetStatus,
        string reason,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = new EquipmentStatusTransitionResult
        {
            Success = false,
            ValidationErrors = new List<string>(),
            TransitionTimestamp = DateTime.UtcNow,
            PublishedEvents = new List<IDomainEvent>()
        };

        _logger.LogInformation("Starting status transition for equipment {EquipmentId} to {TargetStatus}", equipmentId, targetStatus);

        try
        {
            // Get equipment data (simplified for demo)
            var equipmentList = await _equipmentService.GetEquipmentSortedAsync(equipmentId);
            var equipment = equipmentList.FirstOrDefault();
            
            if (equipment == null)
            {
                result.ValidationErrors.Add("Equipment not found");
                return result;
            }

            result.PreviousStatus = equipment.Status;

            // Update equipment status
            equipment.Status = targetStatus;
            equipment.Entry_Date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            await _equipmentService.UpdateLatestEntryAsync(equipment);

            result.CurrentStatus = targetStatus;
            result.Success = true;

            // Publish status updated event
            var statusEvent = new EquipmentUpdatedEvent(
                equipment,
                result.PreviousStatus,
                targetStatus,
                userId,
                null,
                reason);

            await _eventDispatcher.PublishAsync(statusEvent);
            result.PublishedEvents.Add(statusEvent);

            _logger.LogInformation("Status transition completed for equipment {EquipmentId}: {PreviousStatus} → {NewStatus}", 
                equipmentId, result.PreviousStatus, targetStatus);
        }
        catch (Exception ex)
        {
            result.ValidationErrors.Add($"Unexpected error during status transition: {ex.Message}");
            _logger.LogError(ex, "Error during status transition for equipment {EquipmentId}", equipmentId);
        }

        return result;
    }

    public async Task<EquipmentArchiveResult> ArchiveEquipmentAsync(
        IEnumerable<int> equipmentIds,
        string archiveReason,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = new EquipmentArchiveResult
        {
            Success = true,
            ArchivedCount = 0,
            FailedCount = 0,
            ArchiveDetails = new Dictionary<int, string>(),
            Errors = new List<string>(),
            PublishedEvents = new List<IDomainEvent>()
        };

        _logger.LogInformation("Starting archive operation for {Count} equipment items", equipmentIds.Count());

        foreach (var equipmentId in equipmentIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Get equipment data (simplified for demo)
                var equipmentList = await _equipmentService.GetEquipmentSortedAsync(equipmentId);
                var equipment = equipmentList.FirstOrDefault();
                
                if (equipment == null)
                {
                    result.FailedCount++;
                    result.ArchiveDetails[equipmentId] = "Equipment not found";
                    result.Errors.Add($"Equipment {equipmentId} not found");
                    continue;
                }

                // Check if equipment can be archived
                if (equipment.Status == "Hos Bruger")
                {
                    result.FailedCount++;
                    result.ArchiveDetails[equipmentId] = "Cannot archive equipment currently in use";
                    result.Errors.Add($"Equipment {equipmentId} is currently in use and cannot be archived");
                    continue;
                }

                // Update equipment to archived status
                equipment.Status = "Kasseret";
                equipment.Entry_Date = DateTime.UtcNow.ToString("yyyy-MM-dd");
                equipment.Note += $" [Archived: {archiveReason}]";

                await _equipmentService.UpdateLatestEntryAsync(equipment);
                
                result.ArchivedCount++;
                result.ArchiveDetails[equipmentId] = $"Archived successfully. Reason: {archiveReason}";

                // Publish archive event
                var archiveEvent = new EquipmentUpdatedEvent(
                    equipment,
                    equipment.Status,
                    "Kasseret",
                    userId,
                    null,
                    $"Archived: {archiveReason}");

                await _eventDispatcher.PublishAsync(archiveEvent);
                result.PublishedEvents.Add(archiveEvent);
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.ArchiveDetails[equipmentId] = $"Error: {ex.Message}";
                result.Errors.Add($"Error archiving equipment {equipmentId}: {ex.Message}");
                _logger.LogError(ex, "Error archiving equipment {EquipmentId}", equipmentId);
            }
        }

        result.Success = result.FailedCount == 0;

        _logger.LogInformation("Archive operation completed. Archived: {Archived}, Failed: {Failed}", 
            result.ArchivedCount, result.FailedCount);

        return result;
    }
}