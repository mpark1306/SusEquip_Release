using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Implementation of compensation coordinator for managing saga transactions
    /// </summary>
    public class CompensationCoordinatorService : ICompensationCoordinator
    {
        private readonly ILogger<CompensationCoordinatorService> _logger;
        private readonly List<ICompensatableOperation> _operations = new();
        private readonly object _lock = new object();

        public CompensationCoordinatorService(ILogger<CompensationCoordinatorService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyList<ICompensatableOperation> Operations
        {
            get
            {
                lock (_lock)
                {
                    return _operations.ToList();
                }
            }
        }

        public event EventHandler<CompensationStartedEventArgs>? CompensationStarted;
        public event EventHandler<CompensationCompletedEventArgs>? CompensationCompleted;
        public event EventHandler<OperationCompensatedEventArgs>? OperationCompensated;

        public void AddOperation(ICompensatableOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            lock (_lock)
            {
                _operations.Add(operation);
                _logger.LogDebug("Added compensatable operation '{OperationName}' with ID '{OperationId}'", 
                    operation.OperationName, operation.OperationId);
            }
        }

        public async Task<List<object?>> ExecuteAllAsync(CancellationToken cancellationToken = default)
        {
            List<ICompensatableOperation> operationsToExecute;
            
            lock (_lock)
            {
                operationsToExecute = _operations.ToList();
            }

            var results = new List<object?>();

            _logger.LogInformation("Executing {Count} compensatable operations", operationsToExecute.Count);

            foreach (var operation in operationsToExecute)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logger.LogDebug("Executing operation '{OperationName}' with ID '{OperationId}'", 
                        operation.OperationName, operation.OperationId);

                    var result = await operation.ExecuteAsync(cancellationToken);
                    results.Add(result);

                    _logger.LogDebug("Operation '{OperationName}' completed successfully", operation.OperationName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' failed during execution", operation.OperationName);
                    throw;
                }
            }

            _logger.LogInformation("All {Count} operations executed successfully", operationsToExecute.Count);
            return results;
        }

        public async Task<CompensationResult> CompensateAsync(CancellationToken cancellationToken = default)
        {
            List<ICompensatableOperation> operationsToCompensate;
            
            lock (_lock)
            {
                // Only compensate operations that completed successfully
                operationsToCompensate = _operations
                    .Where(op => op.State == CompensatableOperationState.Completed && op.CanCompensate)
                    .Reverse() // Compensate in reverse order
                    .ToList();
            }

            if (!operationsToCompensate.Any())
            {
                _logger.LogInformation("No operations to compensate");
                return new CompensationResult(true);
            }

            _logger.LogWarning("Starting compensation for {Count} operations", operationsToCompensate.Count);
            FireCompensationStarted(operationsToCompensate.Count);

            var errors = new List<string>();
            var warnings = new List<string>();
            int compensatedCount = 0;
            int failedCount = 0;

            foreach (var operation in operationsToCompensate)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logger.LogInformation("Compensating operation '{OperationName}' with ID '{OperationId}'", 
                        operation.OperationName, operation.OperationId);

                    await operation.CompensateAsync(cancellationToken);
                    compensatedCount++;

                    _logger.LogInformation("Operation '{OperationName}' compensated successfully", operation.OperationName);
                    FireOperationCompensated(operation, true);
                }
                catch (Exception ex)
                {
                    failedCount++;
                    var errorMessage = $"Failed to compensate operation '{operation.OperationName}': {ex.Message}";
                    errors.Add(errorMessage);

                    _logger.LogError(ex, "Failed to compensate operation '{OperationName}'", operation.OperationName);
                    FireOperationCompensated(operation, false, ex);

                    // Continue compensating other operations even if one fails
                    warnings.Add($"Continuing compensation despite failure in '{operation.OperationName}'");
                }
            }

            var success = failedCount == 0;
            var result = new CompensationResult(success, errors, warnings)
            {
                CompensatedOperationsCount = compensatedCount,
                FailedCompensationsCount = failedCount
            };

            var logLevel = success ? LogLevel.Information : LogLevel.Error;
            _logger.Log(logLevel, "Compensation completed: {CompensatedCount} succeeded, {FailedCount} failed", 
                compensatedCount, failedCount);

            FireCompensationCompleted(result);
            return result;
        }

        public async Task<(bool Success, List<object?> Results, CompensationResult? CompensationResult)> ExecuteWithCompensationAsync(
            CancellationToken cancellationToken = default)
        {
            List<object?> results;
            CompensationResult? compensationResult = null;

            try
            {
                results = await ExecuteAllAsync(cancellationToken);
                return (true, results, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operation execution failed, starting compensation");

                try
                {
                    compensationResult = await CompensateAsync(cancellationToken);
                }
                catch (Exception compensationEx)
                {
                    _logger.LogCritical(compensationEx, "Compensation failed after operation failure - system may be in inconsistent state");
                    // Return the original execution failure, but note compensation also failed
                    compensationResult = new CompensationResult(false, new List<string> { compensationEx.Message });
                }

                return (false, new List<object?>(), compensationResult);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                var count = _operations.Count;
                _operations.Clear();
                _logger.LogDebug("Cleared {Count} compensatable operations", count);
            }
        }

        private void FireCompensationStarted(int operationCount)
        {
            try
            {
                CompensationStarted?.Invoke(this, new CompensationStartedEventArgs(operationCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing CompensationStarted event");
            }
        }

        private void FireCompensationCompleted(CompensationResult result)
        {
            try
            {
                CompensationCompleted?.Invoke(this, new CompensationCompletedEventArgs(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing CompensationCompleted event");
            }
        }

        private void FireOperationCompensated(ICompensatableOperation operation, bool success, Exception? exception = null)
        {
            try
            {
                OperationCompensated?.Invoke(this, new OperationCompensatedEventArgs(operation, success, exception));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing OperationCompensated event");
            }
        }
    }

    /// <summary>
    /// Factory for creating compensation coordinators and common compensatable operations
    /// </summary>
    public static class CompensationFactory
    {
        /// <summary>
        /// Create a new compensation coordinator
        /// </summary>
        public static ICompensationCoordinator CreateCoordinator(ILogger<CompensationCoordinatorService> logger)
        {
            return new CompensationCoordinatorService(logger);
        }

        /// <summary>
        /// Create a compensatable operation from delegates
        /// </summary>
        public static ICompensatableOperation<T> CreateOperation<T>(
            string operationName,
            Func<CancellationToken, Task<T>> executeFunc,
            Func<CancellationToken, Task> compensateFunc)
        {
            return new DelegateCompensatableOperation<T>(operationName, executeFunc, compensateFunc);
        }

        /// <summary>
        /// Create a compensatable operation from delegates (void return)
        /// </summary>
        public static ICompensatableOperation CreateOperation(
            string operationName,
            Func<CancellationToken, Task> executeFunc,
            Func<CancellationToken, Task> compensateFunc)
        {
            return new DelegateCompensatableOperation(operationName, executeFunc, compensateFunc);
        }
    }

    /// <summary>
    /// Implementation of compensatable operation using delegates
    /// </summary>
    internal class DelegateCompensatableOperation<T> : CompensatableOperationBase<T>
    {
        private readonly Func<CancellationToken, Task<T>> _executeFunc;
        private readonly Func<CancellationToken, Task> _compensateFunc;

        public DelegateCompensatableOperation(
            string operationName,
            Func<CancellationToken, Task<T>> executeFunc,
            Func<CancellationToken, Task> compensateFunc)
            : base(operationName)
        {
            _executeFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            _compensateFunc = compensateFunc ?? throw new ArgumentNullException(nameof(compensateFunc));
        }

        protected override async Task<T> ExecuteTypedOperationAsync(CancellationToken cancellationToken)
        {
            return await _executeFunc(cancellationToken);
        }

        protected override async Task CompensateOperationAsync(CancellationToken cancellationToken)
        {
            await _compensateFunc(cancellationToken);
        }
    }

    /// <summary>
    /// Implementation of compensatable operation using delegates (void return)
    /// </summary>
    internal class DelegateCompensatableOperation : CompensatableOperationBase
    {
        private readonly Func<CancellationToken, Task> _executeFunc;
        private readonly Func<CancellationToken, Task> _compensateFunc;

        public DelegateCompensatableOperation(
            string operationName,
            Func<CancellationToken, Task> executeFunc,
            Func<CancellationToken, Task> compensateFunc)
            : base(operationName)
        {
            _executeFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            _compensateFunc = compensateFunc ?? throw new ArgumentNullException(nameof(compensateFunc));
        }

        protected override async Task<object?> ExecuteOperationAsync(CancellationToken cancellationToken)
        {
            await _executeFunc(cancellationToken);
            return null;
        }

        protected override async Task CompensateOperationAsync(CancellationToken cancellationToken)
        {
            await _compensateFunc(cancellationToken);
        }
    }
}