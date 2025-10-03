using System;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Represents the state of a circuit breaker
    /// </summary>
    public enum CircuitBreakerState
    {
        /// <summary>
        /// Circuit is closed - operations flow through normally
        /// </summary>
        Closed,
        
        /// <summary>
        /// Circuit is open - operations fail immediately to prevent cascading failures
        /// </summary>
        Open,
        
        /// <summary>
        /// Circuit is half-open - testing if the service has recovered
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// Configuration options for circuit breaker behavior
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// Number of consecutive failures before opening the circuit
        /// </summary>
        public int FailureThreshold { get; set; } = 5;
        
        /// <summary>
        /// Time to wait before attempting to close the circuit
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Number of successful operations in half-open state before closing
        /// </summary>
        public int SuccessThreshold { get; set; } = 3;
        
        /// <summary>
        /// Name identifier for this circuit breaker instance
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Exception thrown when circuit breaker is open
    /// </summary>
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException(string circuitName) 
            : base($"Circuit breaker '{circuitName}' is open")
        {
            CircuitName = circuitName;
        }

        public string CircuitName { get; }
    }

    /// <summary>
    /// Circuit breaker pattern implementation for fault tolerance
    /// Prevents cascading failures by failing fast when a service is unavailable
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// Current state of the circuit breaker
        /// </summary>
        CircuitBreakerState State { get; }
        
        /// <summary>
        /// Configuration options for this circuit breaker
        /// </summary>
        CircuitBreakerOptions Options { get; }
        
        /// <summary>
        /// Number of consecutive failures recorded
        /// </summary>
        int FailureCount { get; }
        
        /// <summary>
        /// Number of consecutive successes in half-open state
        /// </summary>
        int SuccessCount { get; }
        
        /// <summary>
        /// Last time the circuit breaker opened
        /// </summary>
        DateTime? LastFailureTime { get; }

        /// <summary>
        /// Execute an operation through the circuit breaker
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="CircuitBreakerOpenException">Thrown when circuit is open</exception>
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
        
        /// <summary>
        /// Execute an operation through the circuit breaker (void return)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <exception cref="CircuitBreakerOpenException">Thrown when circuit is open</exception>
        Task ExecuteAsync(Func<Task> operation);
        
        /// <summary>
        /// Manually reset the circuit breaker to closed state
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Manually trip the circuit breaker to open state
        /// </summary>
        void Trip();

        /// <summary>
        /// Event fired when circuit breaker state changes
        /// </summary>
        event EventHandler<CircuitBreakerStateChangedEventArgs> StateChanged;
    }

    /// <summary>
    /// Event arguments for circuit breaker state change events
    /// </summary>
    public class CircuitBreakerStateChangedEventArgs : EventArgs
    {
        public CircuitBreakerStateChangedEventArgs(
            CircuitBreakerState previousState, 
            CircuitBreakerState newState, 
            string circuitName,
            Exception? lastException = null)
        {
            PreviousState = previousState;
            NewState = newState;
            CircuitName = circuitName;
            LastException = lastException;
            Timestamp = DateTime.UtcNow;
        }

        public CircuitBreakerState PreviousState { get; }
        public CircuitBreakerState NewState { get; }
        public string CircuitName { get; }
        public Exception? LastException { get; }
        public DateTime Timestamp { get; }
    }
}