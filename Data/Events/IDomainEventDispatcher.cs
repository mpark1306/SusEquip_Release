using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Events
{
    /// <summary>
    /// Interface for handling domain events
    /// </summary>
    /// <typeparam name="T">Type of domain event to handle</typeparam>
    public interface IDomainEventHandler<in T> where T : IDomainEvent
    {
        /// <summary>
        /// Handle the domain event asynchronously
        /// </summary>
        /// <param name="domainEvent">The event to handle</param>
        /// <returns>Task representing the async operation</returns>
        Task HandleAsync(T domainEvent);
    }

    /// <summary>
    /// Interface for dispatching domain events to their handlers
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Publish a single domain event to all registered handlers
        /// </summary>
        /// <typeparam name="T">Type of domain event</typeparam>
        /// <param name="domainEvent">The event to publish</param>
        /// <returns>Task representing the async operation</returns>
        Task PublishAsync<T>(T domainEvent) where T : IDomainEvent;
        
        /// <summary>
        /// Publish multiple domain events to all registered handlers
        /// </summary>
        /// <param name="domainEvents">Collection of events to publish</param>
        /// <returns>Task representing the async operation</returns>
        Task PublishAsync(IEnumerable<IDomainEvent> domainEvents);
        
        /// <summary>
        /// Register a handler for a specific event type
        /// </summary>
        /// <typeparam name="T">Type of domain event</typeparam>
        /// <param name="handler">Handler to register</param>
        void RegisterHandler<T>(IDomainEventHandler<T> handler) where T : IDomainEvent;
        
        /// <summary>
        /// Unregister a handler for a specific event type
        /// </summary>
        /// <typeparam name="T">Type of domain event</typeparam>
        /// <param name="handler">Handler to unregister</param>
        void UnregisterHandler<T>(IDomainEventHandler<T> handler) where T : IDomainEvent;
    }

    /// <summary>
    /// In-memory implementation of domain event dispatcher
    /// </summary>
    public class InMemoryDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly Dictionary<Type, List<object>> _handlers;
        private readonly ILogger<InMemoryDomainEventDispatcher> _logger;
        private readonly object _lock = new object();

        public InMemoryDomainEventDispatcher(ILogger<InMemoryDomainEventDispatcher> logger)
        {
            _handlers = new Dictionary<Type, List<object>>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync<T>(T domainEvent) where T : IDomainEvent
        {
            if (domainEvent == null)
            {
                _logger.LogWarning("Attempted to publish null domain event");
                return;
            }

            var eventType = typeof(T);
            List<object>? handlers;
            
            lock (_lock)
            {
                _handlers.TryGetValue(eventType, out handlers);
            }

            if (handlers == null || handlers.Count == 0)
            {
                _logger.LogDebug("No handlers registered for event type {EventType}", eventType.Name);
                return;
            }

            _logger.LogInformation("Publishing event {EventType} with ID {EventId} to {HandlerCount} handlers", 
                eventType.Name, domainEvent.EventId, handlers.Count);

            var tasks = new List<Task>();
            foreach (var handler in handlers)
            {
                if (handler is IDomainEventHandler<T> typedHandler)
                {
                    tasks.Add(HandleEventSafelyAsync(typedHandler, domainEvent));
                }
            }

            try
            {
                await Task.WhenAll(tasks);
                _logger.LogInformation("Successfully published event {EventType} with ID {EventId}", 
                    eventType.Name, domainEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "One or more handlers failed while processing event {EventType} with ID {EventId}", 
                    eventType.Name, domainEvent.EventId);
                // Don't rethrow - we want to be resilient to handler failures
            }
        }

        public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents)
        {
            if (domainEvents == null)
            {
                _logger.LogWarning("Attempted to publish null domain events collection");
                return;
            }

            var tasks = new List<Task>();
            foreach (var domainEvent in domainEvents)
            {
                // Use reflection to call the generic PublishAsync method
                var eventType = domainEvent.GetType();
                var method = GetType().GetMethod(nameof(PublishAsync), new[] { eventType });
                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(eventType);
                    var task = (Task)genericMethod.Invoke(this, new object[] { domainEvent })!;
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        public void RegisterHandler<T>(IDomainEventHandler<T> handler) where T : IDomainEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(T);
            lock (_lock)
            {
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<object>();
                }
                
                if (!_handlers[eventType].Contains(handler))
                {
                    _handlers[eventType].Add(handler);
                    _logger.LogDebug("Registered handler {HandlerType} for event type {EventType}", 
                        handler.GetType().Name, eventType.Name);
                }
            }
        }

        public void UnregisterHandler<T>(IDomainEventHandler<T> handler) where T : IDomainEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(T);
            lock (_lock)
            {
                if (_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType].Remove(handler);
                    _logger.LogDebug("Unregistered handler {HandlerType} for event type {EventType}", 
                        handler.GetType().Name, eventType.Name);
                    
                    if (_handlers[eventType].Count == 0)
                    {
                        _handlers.Remove(eventType);
                    }
                }
            }
        }

        private async Task HandleEventSafelyAsync<T>(IDomainEventHandler<T> handler, T domainEvent) where T : IDomainEvent
        {
            try
            {
                await handler.HandleAsync(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType} with ID {EventId}", 
                    handler.GetType().Name, typeof(T).Name, domainEvent.EventId);
                // Don't rethrow - we want other handlers to continue processing
            }
        }
    }
}