using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Thread-safe implementation of the Circuit Breaker pattern
    /// Provides fault tolerance by preventing cascading failures
    /// </summary>
    public class CircuitBreakerService : ICircuitBreaker
    {
        private readonly ILogger<CircuitBreakerService> _logger;
        private readonly object _lock = new object();
        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private int _failureCount = 0;
        private int _successCount = 0;
        private DateTime? _lastFailureTime;

        public CircuitBreakerService(CircuitBreakerOptions options, ILogger<CircuitBreakerService> logger)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            ValidateOptions();
        }

        public CircuitBreakerState State 
        { 
            get 
            { 
                lock (_lock)
                {
                    return _state;
                }
            }
        }

        public CircuitBreakerOptions Options { get; }

        public int FailureCount
        {
            get
            {
                lock (_lock)
                {
                    return _failureCount;
                }
            }
        }

        public int SuccessCount
        {
            get
            {
                lock (_lock)
                {
                    return _successCount;
                }
            }
        }

        public DateTime? LastFailureTime
        {
            get
            {
                lock (_lock)
                {
                    return _lastFailureTime;
                }
            }
        }

        public event EventHandler<CircuitBreakerStateChangedEventArgs>? StateChanged;

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            CheckState();

            try
            {
                _logger.LogDebug("Executing operation through circuit breaker '{CircuitName}'", Options.Name);
                
                var result = await operation();
                
                OnSuccess();
                return result;
            }
            catch (Exception ex)
            {
                OnFailure(ex);
                throw;
            }
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            CheckState();

            try
            {
                _logger.LogDebug("Executing void operation through circuit breaker '{CircuitName}'", Options.Name);
                
                await operation();
                
                OnSuccess();
            }
            catch (Exception ex)
            {
                OnFailure(ex);
                throw;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                var previousState = _state;
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
                _successCount = 0;
                _lastFailureTime = null;

                _logger.LogInformation("Circuit breaker '{CircuitName}' manually reset to Closed state", Options.Name);
                FireStateChanged(previousState, _state);
            }
        }

        public void Trip()
        {
            lock (_lock)
            {
                var previousState = _state;
                _state = CircuitBreakerState.Open;
                _lastFailureTime = DateTime.UtcNow;

                _logger.LogWarning("Circuit breaker '{CircuitName}' manually tripped to Open state", Options.Name);
                FireStateChanged(previousState, _state);
            }
        }

        private void CheckState()
        {
            lock (_lock)
            {
                // If circuit is closed, allow execution
                if (_state == CircuitBreakerState.Closed)
                    return;

                // If circuit is open, check if timeout has passed
                if (_state == CircuitBreakerState.Open)
                {
                    if (_lastFailureTime.HasValue && 
                        DateTime.UtcNow - _lastFailureTime.Value >= Options.Timeout)
                    {
                        // Transition to half-open
                        var previousState = _state;
                        _state = CircuitBreakerState.HalfOpen;
                        _successCount = 0;
                        
                        _logger.LogInformation("Circuit breaker '{CircuitName}' transitioning to HalfOpen state for testing", Options.Name);
                        FireStateChanged(previousState, _state);
                        return;
                    }

                    // Still in timeout period, throw exception
                    _logger.LogWarning("Circuit breaker '{CircuitName}' is open, operation blocked", Options.Name);
                    throw new CircuitBreakerOpenException(Options.Name);
                }

                // If half-open, allow execution (will be handled by success/failure methods)
            }
        }

        private void OnSuccess()
        {
            lock (_lock)
            {
                _failureCount = 0;

                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _successCount++;
                    
                    if (_successCount >= Options.SuccessThreshold)
                    {
                        // Transition back to closed
                        var previousState = _state;
                        _state = CircuitBreakerState.Closed;
                        _successCount = 0;
                        _lastFailureTime = null;

                        _logger.LogInformation("Circuit breaker '{CircuitName}' recovered and transitioned to Closed state", Options.Name);
                        FireStateChanged(previousState, _state);
                    }
                    else
                    {
                        _logger.LogDebug("Circuit breaker '{CircuitName}' success {SuccessCount}/{SuccessThreshold} in HalfOpen state", 
                            Options.Name, _successCount, Options.SuccessThreshold);
                    }
                }
            }
        }

        private void OnFailure(Exception exception)
        {
            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                _logger.LogWarning(exception, "Circuit breaker '{CircuitName}' recorded failure {FailureCount}/{FailureThreshold}", 
                    Options.Name, _failureCount, Options.FailureThreshold);

                if (_state == CircuitBreakerState.HalfOpen)
                {
                    // Any failure in half-open immediately opens the circuit
                    var previousState = _state;
                    _state = CircuitBreakerState.Open;
                    _successCount = 0;

                    _logger.LogWarning("Circuit breaker '{CircuitName}' failed in HalfOpen state, transitioning to Open", Options.Name);
                    FireStateChanged(previousState, _state, exception);
                }
                else if (_state == CircuitBreakerState.Closed && _failureCount >= Options.FailureThreshold)
                {
                    // Threshold exceeded, open the circuit
                    var previousState = _state;
                    _state = CircuitBreakerState.Open;

                    _logger.LogError("Circuit breaker '{CircuitName}' failure threshold exceeded, transitioning to Open state", Options.Name);
                    FireStateChanged(previousState, _state, exception);
                }
            }
        }

        private void FireStateChanged(CircuitBreakerState previousState, CircuitBreakerState newState, Exception? exception = null)
        {
            try
            {
                StateChanged?.Invoke(this, new CircuitBreakerStateChangedEventArgs(previousState, newState, Options.Name, exception));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing StateChanged event for circuit breaker '{CircuitName}'", Options.Name);
            }
        }

        private void ValidateOptions()
        {
            if (Options.FailureThreshold <= 0)
                throw new ArgumentException("FailureThreshold must be greater than 0", nameof(Options.FailureThreshold));

            if (Options.SuccessThreshold <= 0)
                throw new ArgumentException("SuccessThreshold must be greater than 0", nameof(Options.SuccessThreshold));

            if (Options.Timeout <= TimeSpan.Zero)
                throw new ArgumentException("Timeout must be greater than zero", nameof(Options.Timeout));

            if (string.IsNullOrWhiteSpace(Options.Name))
                Options.Name = $"CircuitBreaker_{Guid.NewGuid():N}";
        }
    }
}