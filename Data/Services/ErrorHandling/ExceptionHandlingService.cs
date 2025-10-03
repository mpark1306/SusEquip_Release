using Microsoft.Extensions.Logging;
using SusEquip.Data.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Interface for centralized exception handling and recovery operations
    /// </summary>
    public interface IExceptionHandlingService
    {
        /// <summary>
        /// Handles and logs an exception with appropriate recovery actions
        /// </summary>
        Task<ExceptionHandlingResult> HandleExceptionAsync(Exception exception, string operation, Dictionary<string, object>? context = null);
        
        /// <summary>
        /// Attempts to recover from a failed operation using predefined strategies
        /// </summary>
        Task<RecoveryResult> AttemptRecoveryAsync(Exception exception, string operation, Func<Task> retryAction, int maxRetries = 3);
        
        /// <summary>
        /// Wraps an operation with comprehensive exception handling and retry logic
        /// </summary>
        Task<T> ExecuteWithHandlingAsync<T>(Func<Task<T>> operation, string operationName, Dictionary<string, object>? context = null);
        
        /// <summary>
        /// Reports critical errors that require immediate attention
        /// </summary>
        Task ReportCriticalErrorAsync(Exception exception, string operation, Dictionary<string, object>? context = null);
    }

    /// <summary>
    /// Centralized service for exception handling, logging, and recovery operations
    /// </summary>
    public class ExceptionHandlingService : IExceptionHandlingService
    {
        private readonly ILogger<ExceptionHandlingService> _logger;
        private readonly ActivitySource _activitySource;
        private static readonly Dictionary<Type, ExceptionHandlingStrategy> _handlingStrategies;

        static ExceptionHandlingService()
        {
            _handlingStrategies = new Dictionary<Type, ExceptionHandlingStrategy>
            {
                { typeof(EquipmentValidationException), new ExceptionHandlingStrategy { AllowRetry = false, NotifyCritical = false } },
                { typeof(EquipmentNotFoundException), new ExceptionHandlingStrategy { AllowRetry = true, NotifyCritical = false, MaxRetries = 2 } },
                { typeof(DuplicateSerialNumberException), new ExceptionHandlingStrategy { AllowRetry = false, NotifyCritical = false } },
                { typeof(DatabaseOperationException), new ExceptionHandlingStrategy { AllowRetry = true, NotifyCritical = true, MaxRetries = 3, DelayBetweenRetries = TimeSpan.FromSeconds(2) } },
                { typeof(BusinessRuleException), new ExceptionHandlingStrategy { AllowRetry = false, NotifyCritical = false } },
                { typeof(TimeoutException), new ExceptionHandlingStrategy { AllowRetry = true, NotifyCritical = false, MaxRetries = 2, DelayBetweenRetries = TimeSpan.FromSeconds(1) } },
                { typeof(UnauthorizedAccessException), new ExceptionHandlingStrategy { AllowRetry = false, NotifyCritical = true } }
            };
        }

        public ExceptionHandlingService(ILogger<ExceptionHandlingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _activitySource = new ActivitySource("SusEquip.ExceptionHandling");
        }

        public async Task<ExceptionHandlingResult> HandleExceptionAsync(
            Exception exception,
            string operation,
            Dictionary<string, object>? context = null)
        {
            using var activity = _activitySource.StartActivity("HandleException");
            activity?.SetTag("operation", operation);
            activity?.SetTag("exception.type", exception.GetType().Name);

            var handlingContext = CreateHandlingContext(exception, operation, context);
            var strategy = GetHandlingStrategy(exception);
            
            // Log the exception appropriately
            await LogExceptionAsync(exception, handlingContext);
            
            // Determine if recovery should be attempted
            var result = new ExceptionHandlingResult
            {
                Exception = exception,
                Operation = operation,
                Context = handlingContext,
                Strategy = strategy,
                ShouldRetry = strategy.AllowRetry,
                MaxRetries = strategy.MaxRetries,
                DelayBetweenRetries = strategy.DelayBetweenRetries,
                RequiresCriticalNotification = strategy.NotifyCritical
            };

            // Report critical errors if needed
            if (strategy.NotifyCritical)
            {
                await ReportCriticalErrorAsync(exception, operation, context);
            }

            return result;
        }

        public async Task<RecoveryResult> AttemptRecoveryAsync(
            Exception exception,
            string operation,
            Func<Task> retryAction,
            int maxRetries = 3)
        {
            using var activity = _activitySource.StartActivity("AttemptRecovery");
            activity?.SetTag("operation", operation);
            activity?.SetTag("maxRetries", maxRetries);

            var strategy = GetHandlingStrategy(exception);
            var effectiveMaxRetries = Math.Min(maxRetries, strategy.MaxRetries);
            
            if (!strategy.AllowRetry)
            {
                _logger.LogWarning("Recovery not allowed for exception type {ExceptionType} in operation {Operation}",
                    exception.GetType().Name, operation);
                    
                return new RecoveryResult
                {
                    Success = false,
                    AttemptsMade = 0,
                    FinalException = exception,
                    RecoveryNotAllowed = true
                };
            }

            var result = new RecoveryResult();
            Exception? lastException = exception;

            for (int attempt = 1; attempt <= effectiveMaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Attempting recovery for operation {Operation}, attempt {Attempt}/{MaxRetries}",
                        operation, attempt, effectiveMaxRetries);

                    // Apply delay before retry (except for first attempt)
                    if (attempt > 1 && strategy.DelayBetweenRetries > TimeSpan.Zero)
                    {
                        await Task.Delay(strategy.DelayBetweenRetries);
                    }

                    await retryAction();
                    
                    _logger.LogInformation("Recovery successful for operation {Operation} on attempt {Attempt}",
                        operation, attempt);
                    
                    result.Success = true;
                    result.AttemptsMade = attempt;
                    return result;
                }
                catch (Exception retryException)
                {
                    lastException = retryException;
                    result.AttemptsMade = attempt;
                    
                    _logger.LogWarning(retryException,
                        "Recovery attempt {Attempt}/{MaxRetries} failed for operation {Operation}",
                        attempt, effectiveMaxRetries, operation);
                }
            }

            _logger.LogError("All recovery attempts failed for operation {Operation} after {Attempts} attempts",
                operation, result.AttemptsMade);
            
            result.Success = false;
            result.FinalException = lastException;
            return result;
        }

        public async Task<T> ExecuteWithHandlingAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            Dictionary<string, object>? context = null)
        {
            using var activity = _activitySource.StartActivity("ExecuteWithHandling");
            activity?.SetTag("operation", operationName);

            try
            {
                return await operation();
            }
            catch (Exception exception)
            {
                var handlingResult = await HandleExceptionAsync(exception, operationName, context);
                
                if (handlingResult.ShouldRetry && handlingResult.MaxRetries > 0)
                {
                    var recoveryResult = await AttemptRecoveryAsync(
                        exception,
                        operationName,
                        async () => { await operation(); },
                        handlingResult.MaxRetries);
                    
                    if (!recoveryResult.Success)
                    {
                        throw recoveryResult.FinalException ?? exception;
                    }
                    
                    // Retry the operation one more time after successful recovery
                    return await operation();
                }
                
                throw; // Re-throw if no retry is allowed or all retries failed
            }
        }

        public async Task ReportCriticalErrorAsync(
            Exception exception,
            string operation,
            Dictionary<string, object>? context = null)
        {
            using var activity = _activitySource.StartActivity("ReportCriticalError");
            
            var criticalContext = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["ExceptionType"] = exception.GetType().Name,
                ["Timestamp"] = DateTime.UtcNow,
                ["MachineName"] = Environment.MachineName,
                ["ProcessId"] = Environment.ProcessId
            };

            if (context != null)
            {
                foreach (var kvp in context)
                {
                    criticalContext[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogCritical(exception,
                "CRITICAL ERROR in operation {Operation}: {Message}. Context: {@CriticalContext}",
                operation, exception.Message, criticalContext);

            // In a production environment, you might want to:
            // - Send alerts to monitoring systems
            // - Notify administrators via email/SMS
            // - Write to Windows Event Log
            // - Send to application performance monitoring (APM) tools
            
            await Task.CompletedTask; // Placeholder for actual notification logic
        }

        private static ExceptionHandlingStrategy GetHandlingStrategy(Exception exception)
        {
            var exceptionType = exception.GetType();
            
            if (_handlingStrategies.TryGetValue(exceptionType, out var strategy))
            {
                return strategy;
            }
            
            // Check for base types
            foreach (var kvp in _handlingStrategies)
            {
                if (kvp.Key.IsAssignableFrom(exceptionType))
                {
                    return kvp.Value;
                }
            }
            
            // Default strategy for unknown exceptions
            return new ExceptionHandlingStrategy
            {
                AllowRetry = false,
                NotifyCritical = true,
                MaxRetries = 0
            };
        }

        private static Dictionary<string, object> CreateHandlingContext(
            Exception exception,
            string operation,
            Dictionary<string, object>? userContext)
        {
            var context = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["ExceptionType"] = exception.GetType().Name,
                ["ExceptionMessage"] = exception.Message,
                ["Timestamp"] = DateTime.UtcNow,
                ["ThreadId"] = Thread.CurrentThread.ManagedThreadId
            };

            if (exception is SusEquipException susEquipEx)
            {
                context["ErrorCode"] = susEquipEx.ErrorCode;
                context["Severity"] = susEquipEx.Severity.ToString();
                
                foreach (var kvp in susEquipEx.ErrorContext)
                {
                    context[$"SusEquip.{kvp.Key}"] = kvp.Value;
                }
            }

            if (userContext != null)
            {
                foreach (var kvp in userContext)
                {
                    context[kvp.Key] = kvp.Value;
                }
            }

            return context;
        }

        private async Task LogExceptionAsync(Exception exception, Dictionary<string, object> context)
        {
            switch (exception)
            {
                case SusEquipException susEquipEx:
                    LogSusEquipException(susEquipEx, context);
                    break;
                    
                case ArgumentNullException:
                case ArgumentException:
                    _logger.LogWarning(exception, "Argument validation error in operation {Operation}. Context: {@Context}",
                        context["Operation"], context);
                    break;
                    
                case TimeoutException:
                    _logger.LogWarning(exception, "Timeout occurred in operation {Operation}. Context: {@Context}",
                        context["Operation"], context);
                    break;
                    
                case UnauthorizedAccessException:
                    _logger.LogWarning(exception, "Unauthorized access in operation {Operation}. Context: {@Context}",
                        context["Operation"], context);
                    break;
                    
                default:
                    _logger.LogError(exception, "Unhandled exception in operation {Operation}. Context: {@Context}",
                        context["Operation"], context);
                    break;
            }
            
            await Task.CompletedTask;
        }

        private void LogSusEquipException(SusEquipException exception, Dictionary<string, object> context)
        {
            var logLevel = exception.Severity switch
            {
                ErrorSeverity.Info => LogLevel.Information,
                ErrorSeverity.Warning => LogLevel.Warning,
                ErrorSeverity.Error => LogLevel.Error,
                ErrorSeverity.Critical => LogLevel.Critical,
                ErrorSeverity.Fatal => LogLevel.Critical,
                _ => LogLevel.Error
            };

            _logger.Log(logLevel, exception,
                "SusEquip exception in operation {Operation}: {ErrorCode} - {Message}. Context: {@Context}",
                context["Operation"], exception.ErrorCode, exception.Message, context);
        }
    }

    /// <summary>
    /// Strategy for handling specific exception types
    /// </summary>
    public class ExceptionHandlingStrategy
    {
        public bool AllowRetry { get; set; }
        public int MaxRetries { get; set; } = 3;
        public TimeSpan DelayBetweenRetries { get; set; } = TimeSpan.FromSeconds(1);
        public bool NotifyCritical { get; set; }
    }

    /// <summary>
    /// Result of exception handling operation
    /// </summary>
    public class ExceptionHandlingResult
    {
        public Exception Exception { get; set; } = null!;
        public string Operation { get; set; } = string.Empty;
        public Dictionary<string, object> Context { get; set; } = new();
        public ExceptionHandlingStrategy Strategy { get; set; } = new();
        public bool ShouldRetry { get; set; }
        public int MaxRetries { get; set; }
        public TimeSpan DelayBetweenRetries { get; set; }
        public bool RequiresCriticalNotification { get; set; }
    }

    /// <summary>
    /// Result of recovery attempt
    /// </summary>
    public class RecoveryResult
    {
        public bool Success { get; set; }
        public int AttemptsMade { get; set; }
        public Exception? FinalException { get; set; }
        public bool RecoveryNotAllowed { get; set; }
    }
}