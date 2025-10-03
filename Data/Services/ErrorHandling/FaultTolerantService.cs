using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Configuration for fault-tolerant operations
    /// </summary>
    public class FaultToleranceOptions
    {
        /// <summary>
        /// Circuit breaker configuration
        /// </summary>
        public CircuitBreakerOptions? CircuitBreakerOptions { get; set; }
        
        /// <summary>
        /// Retry policy configuration
        /// </summary>
        public RetryOptions? RetryOptions { get; set; }
        
        /// <summary>
        /// Whether to enable circuit breaker
        /// </summary>
        public bool UseCircuitBreaker { get; set; } = true;
        
        /// <summary>
        /// Whether to enable retry policy
        /// </summary>
        public bool UseRetryPolicy { get; set; } = true;
        
        /// <summary>
        /// Whether to enable compensation tracking
        /// </summary>
        public bool UseCompensation { get; set; } = false;
        
        /// <summary>
        /// Name identifier for this fault tolerance configuration
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comprehensive fault-tolerant service combining Circuit Breaker, Retry, and Compensation patterns
    /// </summary>
    public interface IFaultTolerantService
    {
        /// <summary>
        /// Execute an operation with configured fault tolerance patterns
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute an operation with configured fault tolerance patterns (void return)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute a compensatable operation with fault tolerance
        /// </summary>
        /// <param name="operation">The compensatable operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        Task<T> ExecuteWithCompensationAsync<T>(ICompensatableOperation<T> operation, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute multiple compensatable operations as a transaction
        /// </summary>
        /// <param name="operations">The operations to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Results of all operations or compensation result if failed</returns>
        Task<(bool Success, object[] Results, CompensationResult? CompensationResult)> ExecuteTransactionAsync(
            ICompensatableOperation[] operations, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the current state of the circuit breaker (if enabled)
        /// </summary>
        CircuitBreakerState? CircuitBreakerState { get; }
        
        /// <summary>
        /// Configuration options for this service
        /// </summary>
        FaultToleranceOptions Options { get; }

        /// <summary>
        /// Event fired when fault tolerance patterns are triggered
        /// </summary>
        event EventHandler<FaultToleranceEventArgs> FaultToleranceTriggered;
    }

    /// <summary>
    /// Implementation of comprehensive fault-tolerant service
    /// </summary>
    public class FaultTolerantService : IFaultTolerantService
    {
        private readonly ILogger<FaultTolerantService> _logger;
        private readonly ICircuitBreaker? _circuitBreaker;
        private readonly IRetryPolicy? _retryPolicy;
        private readonly ICompensationCoordinator? _compensationCoordinator;

        public FaultTolerantService(
            FaultToleranceOptions options,
            ILogger<FaultTolerantService> logger,
            ILogger<CircuitBreakerService>? circuitBreakerLogger = null,
            ILogger<RetryPolicyService>? retryPolicyLogger = null,
            ILogger<CompensationCoordinatorService>? compensationLogger = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            ValidateOptions();

            // Initialize circuit breaker if enabled
            if (Options.UseCircuitBreaker && Options.CircuitBreakerOptions != null)
            {
                if (circuitBreakerLogger == null)
                    throw new ArgumentNullException(nameof(circuitBreakerLogger), "Circuit breaker logger is required when circuit breaker is enabled");
                
                _circuitBreaker = new CircuitBreakerService(Options.CircuitBreakerOptions, circuitBreakerLogger);
                _circuitBreaker.StateChanged += OnCircuitBreakerStateChanged;
            }

            // Initialize retry policy if enabled
            if (Options.UseRetryPolicy && Options.RetryOptions != null)
            {
                if (retryPolicyLogger == null)
                    throw new ArgumentNullException(nameof(retryPolicyLogger), "Retry policy logger is required when retry policy is enabled");
                
                _retryPolicy = new RetryPolicyService(Options.RetryOptions, retryPolicyLogger);
                _retryPolicy.RetryAttempt += OnRetryAttempt;
                _retryPolicy.RetriesExhausted += OnRetriesExhausted;
            }

            // Initialize compensation coordinator if enabled
            if (Options.UseCompensation)
            {
                if (compensationLogger == null)
                    throw new ArgumentNullException(nameof(compensationLogger), "Compensation logger is required when compensation is enabled");
                
                _compensationCoordinator = new CompensationCoordinatorService(compensationLogger);
            }

            _logger.LogInformation("Fault tolerant service '{Name}' initialized with CB:{UseCircuitBreaker}, Retry:{UseRetryPolicy}, Compensation:{UseCompensation}",
                Options.Name, Options.UseCircuitBreaker, Options.UseRetryPolicy, Options.UseCompensation);
        }

        public FaultToleranceOptions Options { get; }

        public CircuitBreakerState? CircuitBreakerState => _circuitBreaker?.State;

        public event EventHandler<FaultToleranceEventArgs>? FaultToleranceTriggered;

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            _logger.LogDebug("Executing operation with fault tolerance '{Name}'", Options.Name);

            // Wrap the operation with fault tolerance patterns
            Func<Task<T>> wrappedOperation = operation;

            // Apply circuit breaker if enabled
            if (_circuitBreaker != null)
            {
                var circuitBreakerOperation = wrappedOperation;
                wrappedOperation = () => _circuitBreaker.ExecuteAsync(circuitBreakerOperation);
            }

            // Apply retry policy if enabled
            if (_retryPolicy != null)
            {
                var retryOperation = wrappedOperation;
                wrappedOperation = () => _retryPolicy.ExecuteAsync(retryOperation, cancellationToken);
            }

            try
            {
                return await wrappedOperation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fault tolerant operation failed for service '{Name}'", Options.Name);
                throw;
            }
        }

        public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            _logger.LogDebug("Executing void operation with fault tolerance '{Name}'", Options.Name);

            // Wrap the operation with fault tolerance patterns
            Func<Task> wrappedOperation = operation;

            // Apply circuit breaker if enabled
            if (_circuitBreaker != null)
            {
                var circuitBreakerOperation = wrappedOperation;
                wrappedOperation = () => _circuitBreaker.ExecuteAsync(circuitBreakerOperation);
            }

            // Apply retry policy if enabled
            if (_retryPolicy != null)
            {
                var retryOperation = wrappedOperation;
                wrappedOperation = () => _retryPolicy.ExecuteAsync(retryOperation, cancellationToken);
            }

            try
            {
                await wrappedOperation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fault tolerant void operation failed for service '{Name}'", Options.Name);
                throw;
            }
        }

        public async Task<T> ExecuteWithCompensationAsync<T>(ICompensatableOperation<T> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (_compensationCoordinator == null)
                throw new InvalidOperationException("Compensation is not enabled for this fault tolerant service");

            _logger.LogDebug("Executing compensatable operation '{OperationName}' with fault tolerance '{Name}'", 
                operation.OperationName, Options.Name);

            _compensationCoordinator.Clear();
            _compensationCoordinator.AddOperation(operation);

            var (success, results, compensationResult) = await _compensationCoordinator.ExecuteWithCompensationAsync(cancellationToken);

            if (!success)
            {
                _logger.LogError("Compensatable operation failed and was compensated for service '{Name}'", Options.Name);
                throw new InvalidOperationException($"Operation failed and was compensated. Compensation success: {compensationResult?.Success}");
            }

            return (T)results[0]!;
        }

        public async Task<(bool Success, object[] Results, CompensationResult? CompensationResult)> ExecuteTransactionAsync(
            ICompensatableOperation[] operations, CancellationToken cancellationToken = default)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            if (_compensationCoordinator == null)
                throw new InvalidOperationException("Compensation is not enabled for this fault tolerant service");

            _logger.LogInformation("Executing transaction with {Count} operations using fault tolerance '{Name}'", 
                operations.Length, Options.Name);

            _compensationCoordinator.Clear();

            foreach (var operation in operations)
            {
                _compensationCoordinator.AddOperation(operation);
            }

            var (success, results, compensationResult) = await _compensationCoordinator.ExecuteWithCompensationAsync(cancellationToken);

            return (success, results.ToArray(), compensationResult);
        }

        private void OnCircuitBreakerStateChanged(object? sender, CircuitBreakerStateChangedEventArgs e)
        {
            _logger.LogWarning("Circuit breaker state changed from {PreviousState} to {NewState} for service '{Name}'",
                e.PreviousState, e.NewState, Options.Name);

            FireFaultToleranceTriggered("CircuitBreakerStateChanged", $"State: {e.NewState}", e.LastException);
        }

        private void OnRetryAttempt(object? sender, RetryAttemptEventArgs e)
        {
            _logger.LogWarning("Retry attempt {Attempt} with delay {Delay}ms for service '{Name}'",
                e.Context.AttemptNumber, e.Delay.TotalMilliseconds, Options.Name);

            FireFaultToleranceTriggered("RetryAttempt", $"Attempt: {e.Context.AttemptNumber}", e.Context.LastException);
        }

        private void OnRetriesExhausted(object? sender, RetryExhaustedEventArgs e)
        {
            _logger.LogError("Retries exhausted after {Attempts} attempts for service '{Name}'",
                e.Context.AttemptNumber, Options.Name);

            FireFaultToleranceTriggered("RetriesExhausted", $"Final attempt: {e.Context.AttemptNumber}", e.Context.LastException);
        }

        private void FireFaultToleranceTriggered(string eventType, string details, Exception? exception = null)
        {
            try
            {
                FaultToleranceTriggered?.Invoke(this, new FaultToleranceEventArgs(eventType, details, Options.Name, exception));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing FaultToleranceTriggered event");
            }
        }

        private void ValidateOptions()
        {
            if (Options.UseCircuitBreaker && Options.CircuitBreakerOptions == null)
                throw new ArgumentException("CircuitBreakerOptions must be provided when UseCircuitBreaker is true");

            if (Options.UseRetryPolicy && Options.RetryOptions == null)
                throw new ArgumentException("RetryOptions must be provided when UseRetryPolicy is true");

            if (string.IsNullOrWhiteSpace(Options.Name))
                Options.Name = $"FaultTolerantService_{Guid.NewGuid():N}";
        }
    }

    /// <summary>
    /// Event arguments for fault tolerance events
    /// </summary>
    public class FaultToleranceEventArgs : EventArgs
    {
        public FaultToleranceEventArgs(string eventType, string details, string serviceName, Exception? exception = null)
        {
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            Details = details ?? throw new ArgumentNullException(nameof(details));
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            Exception = exception;
            Timestamp = DateTime.UtcNow;
        }

        public string EventType { get; }
        public string Details { get; }
        public string ServiceName { get; }
        public Exception? Exception { get; }
        public DateTime Timestamp { get; }
    }
}