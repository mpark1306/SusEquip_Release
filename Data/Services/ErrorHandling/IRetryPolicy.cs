using System;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Configuration for retry behavior
    /// </summary>
    public class RetryOptions
    {
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxAttempts { get; set; } = 3;
        
        /// <summary>
        /// Base delay between retries
        /// </summary>
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
        
        /// <summary>
        /// Maximum delay between retries
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Multiplier for exponential backoff
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2.0;
        
        /// <summary>
        /// Whether to add jitter to prevent thundering herd
        /// </summary>
        public bool UseJitter { get; set; } = true;
        
        /// <summary>
        /// Name identifier for this retry policy
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Context information for retry attempts
    /// </summary>
    public class RetryContext
    {
        public RetryContext(int attemptNumber, TimeSpan totalElapsed, Exception? lastException = null)
        {
            AttemptNumber = attemptNumber;
            TotalElapsed = totalElapsed;
            LastException = lastException;
        }

        /// <summary>
        /// Current attempt number (1-based)
        /// </summary>
        public int AttemptNumber { get; }
        
        /// <summary>
        /// Total time elapsed since first attempt
        /// </summary>
        public TimeSpan TotalElapsed { get; }
        
        /// <summary>
        /// Exception from the last failed attempt
        /// </summary>
        public Exception? LastException { get; }
    }

    /// <summary>
    /// Delegate for determining if an exception should trigger a retry
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="context">Current retry context</param>
    /// <returns>True if the operation should be retried</returns>
    public delegate bool ShouldRetryPredicate(Exception exception, RetryContext context);

    /// <summary>
    /// Delegate for calculating delay before next retry attempt
    /// </summary>
    /// <param name="context">Current retry context</param>
    /// <param name="options">Retry options</param>
    /// <returns>Delay before next attempt</returns>
    public delegate TimeSpan DelayCalculator(RetryContext context, RetryOptions options);

    /// <summary>
    /// Retry policy for handling transient failures with various backoff strategies
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Configuration options for this retry policy
        /// </summary>
        RetryOptions Options { get; }

        /// <summary>
        /// Execute an operation with retry logic
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute an operation with retry logic (void return)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute an operation with retry logic and custom should retry predicate
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="shouldRetry">Custom predicate to determine if retry should occur</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation, 
            ShouldRetryPredicate shouldRetry, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute an operation with retry logic and custom should retry predicate (void return)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="shouldRetry">Custom predicate to determine if retry should occur</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExecuteAsync(
            Func<Task> operation, 
            ShouldRetryPredicate shouldRetry, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Event fired before each retry attempt
        /// </summary>
        event EventHandler<RetryAttemptEventArgs> RetryAttempt;

        /// <summary>
        /// Event fired when all retries are exhausted
        /// </summary>
        event EventHandler<RetryExhaustedEventArgs> RetriesExhausted;
    }

    /// <summary>
    /// Event arguments for retry attempt events
    /// </summary>
    public class RetryAttemptEventArgs : EventArgs
    {
        public RetryAttemptEventArgs(RetryContext context, TimeSpan delay, string policyName)
        {
            Context = context;
            Delay = delay;
            PolicyName = policyName;
            Timestamp = DateTime.UtcNow;
        }

        public RetryContext Context { get; }
        public TimeSpan Delay { get; }
        public string PolicyName { get; }
        public DateTime Timestamp { get; }
    }

    /// <summary>
    /// Event arguments for retry exhausted events
    /// </summary>
    public class RetryExhaustedEventArgs : EventArgs
    {
        public RetryExhaustedEventArgs(RetryContext context, string policyName)
        {
            Context = context;
            PolicyName = policyName;
            Timestamp = DateTime.UtcNow;
        }

        public RetryContext Context { get; }
        public string PolicyName { get; }
        public DateTime Timestamp { get; }
    }

    /// <summary>
    /// Static factory for common retry predicates
    /// </summary>
    public static class RetryPredicates
    {
        /// <summary>
        /// Always retry (up to max attempts)
        /// </summary>
        public static readonly ShouldRetryPredicate Always = (ex, ctx) => true;

        /// <summary>
        /// Never retry
        /// </summary>
        public static readonly ShouldRetryPredicate Never = (ex, ctx) => false;

        /// <summary>
        /// Retry only for specific exception types
        /// </summary>
        public static ShouldRetryPredicate ForExceptionTypes(params Type[] exceptionTypes)
        {
            return (ex, ctx) =>
            {
                var exceptionType = ex.GetType();
                foreach (var type in exceptionTypes)
                {
                    if (type.IsAssignableFrom(exceptionType))
                        return true;
                }
                return false;
            };
        }

        /// <summary>
        /// Retry for transient exceptions (timeouts, network issues, etc.)
        /// </summary>
        public static readonly ShouldRetryPredicate ForTransientExceptions = (ex, ctx) =>
        {
            return ex is TimeoutException ||
                   ex is TaskCanceledException ||
                   (ex is HttpRequestException) ||
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase);
        };
    }

    /// <summary>
    /// Static factory for common delay calculators
    /// </summary>
    public static class DelayCalculators
    {
        /// <summary>
        /// Fixed delay between attempts
        /// </summary>
        public static readonly DelayCalculator Fixed = (ctx, options) => options.BaseDelay;

        /// <summary>
        /// Exponential backoff delay
        /// </summary>
        public static readonly DelayCalculator ExponentialBackoff = (ctx, options) =>
        {
            var delay = TimeSpan.FromMilliseconds(
                options.BaseDelay.TotalMilliseconds * Math.Pow(options.BackoffMultiplier, ctx.AttemptNumber - 1));
            
            if (delay > options.MaxDelay)
                delay = options.MaxDelay;

            if (options.UseJitter)
            {
                var random = new Random();
                var jitter = delay.TotalMilliseconds * 0.1 * (random.NextDouble() - 0.5);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds + jitter);
            }

            return delay;
        };

        /// <summary>
        /// Linear backoff delay
        /// </summary>
        public static readonly DelayCalculator LinearBackoff = (ctx, options) =>
        {
            var delay = TimeSpan.FromMilliseconds(options.BaseDelay.TotalMilliseconds * ctx.AttemptNumber);
            
            if (delay > options.MaxDelay)
                delay = options.MaxDelay;

            return delay;
        };
    }
}