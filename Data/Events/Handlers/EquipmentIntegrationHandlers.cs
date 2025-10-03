using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Events;
using SusEquip.Data.Events.Equipment;
using SusEquip.Data.Services;

namespace SusEquip.Data.Events.Handlers
{
    /// <summary>
    /// Handler that invalidates caches when equipment is modified
    /// </summary>
    public class EquipmentCacheInvalidationHandler : 
        IDomainEventHandler<EquipmentCreatedEvent>,
        IDomainEventHandler<EquipmentUpdatedEvent>,
        IDomainEventHandler<EquipmentDeletedEvent>
    {
        private readonly DashboardCacheService _cacheService;
        private readonly ILogger<EquipmentCacheInvalidationHandler> _logger;

        public EquipmentCacheInvalidationHandler(
            DashboardCacheService cacheService,
            ILogger<EquipmentCacheInvalidationHandler> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(EquipmentCreatedEvent domainEvent)
        {
            await InvalidateRelevantCaches(domainEvent.EquipmentType, domainEvent.EventId);
        }

        public async Task HandleAsync(EquipmentUpdatedEvent domainEvent)
        {
            await InvalidateRelevantCaches(domainEvent.EquipmentType, domainEvent.EventId);
        }

        public async Task HandleAsync(EquipmentDeletedEvent domainEvent)
        {
            await InvalidateRelevantCaches(domainEvent.EquipmentType, domainEvent.EventId);
        }

        private async Task InvalidateRelevantCaches(string equipmentType, Guid eventId)
        {
            try
            {
                _logger.LogDebug("Invalidating caches for equipment type {EquipmentType} due to event {EventId}", 
                    equipmentType, eventId);

                // Clear dashboard cache since equipment counts may have changed
                _cacheService.ClearCache();

                // In a real implementation, you might:
                // - Clear specific cache entries based on equipment type
                // - Update cache with new data instead of just clearing
                // - Notify other cache instances in a distributed system

                _logger.LogInformation("Successfully invalidated caches for equipment type {EquipmentType} due to event {EventId}", 
                    equipmentType, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate caches for equipment type {EquipmentType} due to event {EventId}", 
                    equipmentType, eventId);
                // Don't rethrow - cache invalidation failure shouldn't break the operation
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler that sends notifications when equipment events occur
    /// </summary>
    public class EquipmentNotificationHandler : 
        IDomainEventHandler<EquipmentCreatedEvent>,
        IDomainEventHandler<EquipmentDeletedEvent>,
        IDomainEventHandler<EquipmentValidationFailedEvent>
    {
        private readonly ILogger<EquipmentNotificationHandler> _logger;

        public EquipmentNotificationHandler(ILogger<EquipmentNotificationHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(EquipmentCreatedEvent domainEvent)
        {
            _logger.LogInformation(
                "NOTIFICATION: New equipment created - {PCName} ({InstNo}) by {TriggeredBy}",
                domainEvent.PCName,
                domainEvent.InstNo,
                domainEvent.TriggeredBy);

            // In a real implementation, you might:
            // - Send email notifications to administrators
            // - Send Slack/Teams messages
            // - Update dashboard notifications
            // - Trigger automated workflows

            await Task.CompletedTask;
        }

        public async Task HandleAsync(EquipmentDeletedEvent domainEvent)
        {
            if (domainEvent.IsHardDelete)
            {
                _logger.LogWarning(
                    "NOTIFICATION: Equipment permanently deleted - {PCName} ({InstNo}) by {TriggeredBy}. Reason: {Reason}",
                    domainEvent.PCName,
                    domainEvent.InstNo,
                    domainEvent.TriggeredBy,
                    domainEvent.Reason);

                // Send high-priority notification for permanent deletions
            }
            else
            {
                _logger.LogInformation(
                    "NOTIFICATION: Equipment soft deleted - {PCName} ({InstNo}) by {TriggeredBy}. Reason: {Reason}",
                    domainEvent.PCName,
                    domainEvent.InstNo,
                    domainEvent.TriggeredBy,
                    domainEvent.Reason);
            }

            await Task.CompletedTask;
        }

        public async Task HandleAsync(EquipmentValidationFailedEvent domainEvent)
        {
            _logger.LogWarning(
                "NOTIFICATION: Equipment validation failed - {PCName} ({InstNo}). Errors: {Errors}",
                domainEvent.PCName,
                domainEvent.InstNo,
                string.Join(", ", domainEvent.ValidationErrors));

            // In a real implementation, you might:
            // - Send urgent notifications to data stewards
            // - Create Jira tickets for data quality issues
            // - Trigger data correction workflows

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler that updates metrics and analytics when equipment events occur
    /// </summary>
    public class EquipmentMetricsHandler : 
        IDomainEventHandler<EquipmentCreatedEvent>,
        IDomainEventHandler<EquipmentUpdatedEvent>,
        IDomainEventHandler<EquipmentDeletedEvent>,
        IDomainEventHandler<BulkEquipmentOperationCompletedEvent>
    {
        private readonly ILogger<EquipmentMetricsHandler> _logger;

        public EquipmentMetricsHandler(ILogger<EquipmentMetricsHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(EquipmentCreatedEvent domainEvent)
        {
            _logger.LogDebug("METRICS: Recording equipment creation for type {EquipmentType}", 
                domainEvent.EquipmentType);

            // In a real implementation, you might:
            // - Update Prometheus metrics
            // - Send to Application Insights
            // - Update internal counters
            // - Record performance metrics

            await Task.CompletedTask;
        }

        public async Task HandleAsync(EquipmentUpdatedEvent domainEvent)
        {
            _logger.LogDebug("METRICS: Recording equipment update for type {EquipmentType}", 
                domainEvent.EquipmentType);

            await Task.CompletedTask;
        }

        public async Task HandleAsync(EquipmentDeletedEvent domainEvent)
        {
            _logger.LogDebug("METRICS: Recording equipment deletion for type {EquipmentType}", 
                domainEvent.EquipmentType);

            await Task.CompletedTask;
        }

        public async Task HandleAsync(BulkEquipmentOperationCompletedEvent domainEvent)
        {
            _logger.LogInformation("METRICS: Recording bulk operation - {OperationType}: {SuccessfulItems}/{TotalItems} successful",
                domainEvent.OperationType,
                domainEvent.SuccessfulItems,
                domainEvent.TotalItems);

            // Record bulk operation performance metrics
            var successRate = domainEvent.TotalItems > 0 
                ? (double)domainEvent.SuccessfulItems / domainEvent.TotalItems * 100 
                : 0;

            _logger.LogInformation("METRICS: Bulk operation success rate: {SuccessRate:F1}%", successRate);

            await Task.CompletedTask;
        }
    }
}