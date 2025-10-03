using System;

namespace SusEquip.Data.Events
{
    /// <summary>
    /// Base interface for all domain events in the equipment management system
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        Guid EventId { get; }
        
        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        DateTime OccurredAt { get; }
        
        /// <summary>
        /// Version of the event schema for backward compatibility
        /// </summary>
        int Version { get; }
        
        /// <summary>
        /// User or system that triggered this event
        /// </summary>
        string TriggeredBy { get; }
        
        /// <summary>
        /// Optional correlation ID for tracking related events
        /// </summary>
        string? CorrelationId { get; }
    }
    
    /// <summary>
    /// Base implementation for domain events
    /// </summary>
    public abstract class DomainEventBase : IDomainEvent
    {
        protected DomainEventBase(string triggeredBy, string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            Version = 1;
            TriggeredBy = triggeredBy ?? throw new ArgumentNullException(nameof(triggeredBy));
            CorrelationId = correlationId;
        }
        
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public virtual int Version { get; protected set; }
        public string TriggeredBy { get; }
        public string? CorrelationId { get; }
    }
}