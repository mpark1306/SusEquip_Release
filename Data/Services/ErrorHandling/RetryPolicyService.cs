using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Implementation of retry policy with configurable backoff strategies
    /// </summary>
    public class RetryPolicyService : IRetryPolicy
    {
        private readonly ILogger<RetryPolicyService> _logger;
        private readonly ShouldRetryPredicate _defaultShouldRetry;
        private readonly DelayCalculator _delayCalculator;

        public RetryPolicyService(
            RetryOptions options, 
            ILogger<RetryPolicyService> logger,
            ShouldRetryPredicate? defaultShouldRetry = null,
            DelayCalculator? delayCalculator = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultShouldRetry = defaultShouldRetry ?? RetryPredicates.ForTransientExceptions;
            _delayCalculator = delayCalculator ?? DelayCalculators.ExponentialBackoff;
            
            ValidateOptions();
        }

        public RetryOptions Options { get; }

        public event EventHandler<RetryAttemptEventArgs>? RetryAttempt;
        public event EventHandler<RetryExhaustedEventArgs>? RetriesExhausted;

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(operation, _defaultShouldRetry, cancellationToken);
        }

        public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(operation, _defaultShouldRetry, cancellationToken);
        }

        public async Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation, 
            ShouldRetryPredicate shouldRetry, 
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (shouldRetry == null)
                throw new ArgumentNullException(nameof(shouldRetry));

            var stopwatch = Stopwatch.StartNew();
            Exception? lastException = null;

            for (int attempt = 1; attempt <= Options.MaxAttempts; attempt++)
            {
                var context = new RetryContext(attempt, stopwatch.Elapsed, lastException);

                try
                {
                    _logger.LogDebug("Executing operation attempt {Attempt}/{MaxAttempts} with retry policy '{PolicyName}'", 
                        attempt, Options.MaxAttempts, Options.Name);

                    var result = await operation();
                    
                    if (attempt > 1)
                    {
                        _logger.LogInformation("Operation succeeded on attempt {Attempt}/{MaxAttempts} with retry policy '{PolicyName}'", 
                            attempt, Options.MaxAttempts, Options.Name);
                    }
                    
                    return result;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
                {
                    lastException = ex;
                    
                    _logger.LogWarning(ex, "Operation failed on attempt {Attempt}/{MaxAttempts} with retry policy '{PolicyName}'", 
                        attempt, Options.MaxAttempts, Options.Name);

                    // Check if we should retry
                    if (attempt == Options.MaxAttempts || !shouldRetry(ex, context))
                    {
                        _logger.LogError("Retries exhausted for retry policy '{PolicyName}' after {Attempts} attempts", 
                            Options.Name, attempt);
                        
                        FireRetriesExhausted(context);
                        throw;
                    }

                    // Calculate delay and wait
                    var delay = _delayCalculator(context, Options);
                    
                    _logger.LogDebug("Waiting {Delay}ms before retry attempt {NextAttempt} with retry policy '{PolicyName}'", 
                        delay.TotalMilliseconds, attempt + 1, Options.Name);
                    
                    FireRetryAttempt(context, delay);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Retry operation cancelled for retry policy '{PolicyName}'", Options.Name);
                        throw;
                    }
                }
            }

            // This should never be reached, but just in case
            throw lastException ?? new InvalidOperationException("Unexpected retry policy state");
        }

        public async Task ExecuteAsync(
            Func<Task> operation, 
            ShouldRetryPredicate shouldRetry, 
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (shouldRetry == null)
                throw new ArgumentNullException(nameof(shouldRetry));

            var stopwatch = Stopwatch.StartNew();
            Exception? lastException = null;

            for (int attempt = 1; attempt <= Options.MaxAttempts; attempt++)
            {
                var context = new RetryContext(attempt, stopwatch.Elapsed, lastException);

                try
                {
                    _logger.LogDebug("Executing void operation attempt {Attempt}/{MaxAttempts} with retry policy '{PolicyName}'", 
                        attempt, Options.MaxAttempts, Options.Name);

                    await operation();
                    
                    if (attempt > 1)
                    {
                        _logger.LogInformation("Void operation succeeded on attempt {Attempt}/{MaxAttempts} with retry policy '{PolicyName}'", 
                            attempt, Options.MaxAttempts, Options.Name);
                    }
                    
                    return;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
                {
                    lastException = ex;
                    
                    _logger.LogWarning(ex, "Void operation failed on attempt {Attempt}/{MaxAttempts} with retry policy '{PolicyName}'", 
                        attempt, Options.MaxAttempts, Options.Name);

                    // Check if we should retry
                    if (attempt == Options.MaxAttempts || !shouldRetry(ex, context))
                    {
                        _logger.LogError("Retries exhausted for retry policy '{PolicyName}' after {Attempts} attempts", 
                            Options.Name, attempt);
                        
                        FireRetriesExhausted(context);
                        throw;
                    }

                    // Calculate delay and wait
                    var delay = _delayCalculator(context, Options);
                    
                    _logger.LogDebug("Waiting {Delay}ms before retry attempt {NextAttempt} with retry policy '{PolicyName}'", 
                        delay.TotalMilliseconds, attempt + 1, Options.Name);
                    
                    FireRetryAttempt(context, delay);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Retry void operation cancelled for retry policy '{PolicyName}'", Options.Name);
                        throw;
                    }
                }
            }

            // This should never be reached, but just in case
            throw lastException ?? new InvalidOperationException("Unexpected retry policy state");
        }

        private void FireRetryAttempt(RetryContext context, TimeSpan delay)
        {
            try
            {
                RetryAttempt?.Invoke(this, new RetryAttemptEventArgs(context, delay, Options.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing RetryAttempt event for retry policy '{PolicyName}'", Options.Name);
            }
        }

        private void FireRetriesExhausted(RetryContext context)
        {
            try
            {
                RetriesExhausted?.Invoke(this, new RetryExhaustedEventArgs(context, Options.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing RetriesExhausted event for retry policy '{PolicyName}'", Options.Name);
            }
        }

        private void ValidateOptions()
        {
            if (Options.MaxAttempts <= 0)
                throw new ArgumentException("MaxAttempts must be greater than 0", nameof(Options.MaxAttempts));

            if (Options.BaseDelay < TimeSpan.Zero)
                throw new ArgumentException("BaseDelay cannot be negative", nameof(Options.BaseDelay));

            if (Options.MaxDelay < Options.BaseDelay)
                throw new ArgumentException("MaxDelay cannot be less than BaseDelay", nameof(Options.MaxDelay));

            if (Options.BackoffMultiplier <= 0)
                throw new ArgumentException("BackoffMultiplier must be greater than 0", nameof(Options.BackoffMultiplier));

            if (string.IsNullOrWhiteSpace(Options.Name))
                Options.Name = $"RetryPolicy_{Guid.NewGuid():N}";
        }
    }

    /// <summary>
    /// Factory for creating commonly used retry policies
    /// </summary>
    public static class RetryPolicyFactory
    {
        /// <summary>
        /// Create a retry policy with exponential backoff for transient exceptions
        /// </summary>
        public static IRetryPolicy CreateExponentialBackoff(
            ILogger<RetryPolicyService> logger,
            int maxAttempts = 3,
            TimeSpan? baseDelay = null,
            string name = "ExponentialBackoff")
        {
            var options = new RetryOptions
            {
                MaxAttempts = maxAttempts,
                BaseDelay = baseDelay ?? TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(30),
                BackoffMultiplier = 2.0,
                UseJitter = true,
                Name = name
            };

            return new RetryPolicyService(
                options, 
                logger,
                RetryPredicates.ForTransientExceptions,
                DelayCalculators.ExponentialBackoff);
        }

        /// <summary>
        /// Create a retry policy with fixed delay for transient exceptions
        /// </summary>
        public static IRetryPolicy CreateFixedDelay(
            ILogger<RetryPolicyService> logger,
            int maxAttempts = 3,
            TimeSpan? delay = null,
            string name = "FixedDelay")
        {
            var options = new RetryOptions
            {
                MaxAttempts = maxAttempts,
                BaseDelay = delay ?? TimeSpan.FromSeconds(1),
                MaxDelay = delay ?? TimeSpan.FromSeconds(1),
                BackoffMultiplier = 1.0,
                UseJitter = false,
                Name = name
            };

            return new RetryPolicyService(
                options, 
                logger,
                RetryPredicates.ForTransientExceptions,
                DelayCalculators.Fixed);
        }

        /// <summary>
        /// Create a retry policy with linear backoff for transient exceptions
        /// </summary>
        public static IRetryPolicy CreateLinearBackoff(
            ILogger<RetryPolicyService> logger,
            int maxAttempts = 3,
            TimeSpan? baseDelay = null,
            string name = "LinearBackoff")
        {
            var options = new RetryOptions
            {
                MaxAttempts = maxAttempts,
                BaseDelay = baseDelay ?? TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(30),
                BackoffMultiplier = 1.0,
                UseJitter = false,
                Name = name
            };

            return new RetryPolicyService(
                options, 
                logger,
                RetryPredicates.ForTransientExceptions,
                DelayCalculators.LinearBackoff);
        }
    }
}