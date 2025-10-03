using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.ErrorHandling
{
    /// <summary>
    /// Represents an operation that can be compensated (rolled back)
    /// </summary>
    public interface ICompensatableOperation
    {
        /// <summary>
        /// Unique identifier for this operation
        /// </summary>
        string OperationId { get; }
        
        /// <summary>
        /// Name/description of the operation
        /// </summary>
        string OperationName { get; }
        
        /// <summary>
        /// Execute the primary operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        Task<object?> ExecuteAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute compensation/rollback logic
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the compensation operation</returns>
        Task CompensateAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Indicates if this operation supports compensation
        /// </summary>
        bool CanCompensate { get; }
        
        /// <summary>
        /// Indicates the current state of the operation
        /// </summary>
        CompensatableOperationState State { get; }
    }

    /// <summary>
    /// Generic version of compensatable operation with typed result
    /// </summary>
    /// <typeparam name="T">Type of operation result</typeparam>
    public interface ICompensatableOperation<T> : ICompensatableOperation
    {
        /// <summary>
        /// Execute the primary operation with typed result
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Typed result of the operation</returns>
        new Task<T> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// State of a compensatable operation
    /// </summary>
    public enum CompensatableOperationState
    {
        /// <summary>
        /// Operation not yet started
        /// </summary>
        NotStarted,
        
        /// <summary>
        /// Operation is currently executing
        /// </summary>
        Executing,
        
        /// <summary>
        /// Operation completed successfully
        /// </summary>
        Completed,
        
        /// <summary>
        /// Operation failed
        /// </summary>
        Failed,
        
        /// <summary>
        /// Operation is being compensated
        /// </summary>
        Compensating,
        
        /// <summary>
        /// Operation was successfully compensated
        /// </summary>
        Compensated,
        
        /// <summary>
        /// Compensation failed
        /// </summary>
        CompensationFailed
    }

    /// <summary>
    /// Result of a compensation execution
    /// </summary>
    public class CompensationResult
    {
        public CompensationResult(bool success, List<string>? errors = null, List<string>? warnings = null)
        {
            Success = success;
            Errors = errors ?? new List<string>();
            Warnings = warnings ?? new List<string>();
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Whether the compensation was successful
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Any errors that occurred during compensation
        /// </summary>
        public List<string> Errors { get; }
        
        /// <summary>
        /// Any warnings that occurred during compensation
        /// </summary>
        public List<string> Warnings { get; }
        
        /// <summary>
        /// When the compensation completed
        /// </summary>
        public DateTime CompletedAt { get; }
        
        /// <summary>
        /// Total number of operations that were compensated
        /// </summary>
        public int CompensatedOperationsCount { get; set; }
        
        /// <summary>
        /// Total number of operations that failed compensation
        /// </summary>
        public int FailedCompensationsCount { get; set; }
    }

    /// <summary>
    /// Coordinates compensation for multiple operations (Saga pattern)
    /// </summary>
    public interface ICompensationCoordinator
    {
        /// <summary>
        /// Add an operation to be tracked for compensation
        /// </summary>
        /// <param name="operation">The operation to track</param>
        void AddOperation(ICompensatableOperation operation);
        
        /// <summary>
        /// Execute all registered operations in sequence
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Results of all operations</returns>
        Task<List<object?>> ExecuteAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Compensate all completed operations in reverse order
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the compensation process</returns>
        Task<CompensationResult> CompensateAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute all operations and automatically compensate on failure
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Results of all operations, or compensation result if failed</returns>
        Task<(bool Success, List<object?> Results, CompensationResult? CompensationResult)> ExecuteWithCompensationAsync(
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Clear all registered operations
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Get all registered operations
        /// </summary>
        IReadOnlyList<ICompensatableOperation> Operations { get; }

        /// <summary>
        /// Event fired when compensation starts
        /// </summary>
        event EventHandler<CompensationStartedEventArgs> CompensationStarted;
        
        /// <summary>
        /// Event fired when compensation completes
        /// </summary>
        event EventHandler<CompensationCompletedEventArgs> CompensationCompleted;
        
        /// <summary>
        /// Event fired when individual operation compensation occurs
        /// </summary>
        event EventHandler<OperationCompensatedEventArgs> OperationCompensated;
    }

    /// <summary>
    /// Base implementation of compensatable operation
    /// </summary>
    public abstract class CompensatableOperationBase : ICompensatableOperation
    {
        protected CompensatableOperationBase(string operationName)
        {
            OperationId = Guid.NewGuid().ToString();
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            State = CompensatableOperationState.NotStarted;
        }

        public string OperationId { get; }
        public string OperationName { get; }
        public virtual bool CanCompensate => true;
        public CompensatableOperationState State { get; protected set; }

        public async Task<object?> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            State = CompensatableOperationState.Executing;
            
            try
            {
                var result = await ExecuteOperationAsync(cancellationToken);
                State = CompensatableOperationState.Completed;
                return result;
            }
            catch
            {
                State = CompensatableOperationState.Failed;
                throw;
            }
        }

        public async Task CompensateAsync(CancellationToken cancellationToken = default)
        {
            if (!CanCompensate)
                throw new InvalidOperationException($"Operation '{OperationName}' does not support compensation");

            if (State != CompensatableOperationState.Completed)
                return; // Nothing to compensate

            State = CompensatableOperationState.Compensating;
            
            try
            {
                await CompensateOperationAsync(cancellationToken);
                State = CompensatableOperationState.Compensated;
            }
            catch
            {
                State = CompensatableOperationState.CompensationFailed;
                throw;
            }
        }

        /// <summary>
        /// Override this method to implement the primary operation
        /// </summary>
        protected abstract Task<object?> ExecuteOperationAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// Override this method to implement compensation logic
        /// </summary>
        protected abstract Task CompensateOperationAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Generic base implementation of compensatable operation
    /// </summary>
    /// <typeparam name="T">Type of operation result</typeparam>
    public abstract class CompensatableOperationBase<T> : CompensatableOperationBase, ICompensatableOperation<T>
    {
        protected CompensatableOperationBase(string operationName) : base(operationName)
        {
        }

        public new async Task<T> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.ExecuteAsync(cancellationToken);
            return (T)(result ?? throw new InvalidOperationException("Operation returned null result"));
        }

        protected sealed override async Task<object?> ExecuteOperationAsync(CancellationToken cancellationToken)
        {
            return await ExecuteTypedOperationAsync(cancellationToken);
        }

        /// <summary>
        /// Override this method to implement the primary operation with typed result
        /// </summary>
        protected abstract Task<T> ExecuteTypedOperationAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Event arguments for compensation events
    /// </summary>
    public class CompensationStartedEventArgs : EventArgs
    {
        public CompensationStartedEventArgs(int operationCount)
        {
            OperationCount = operationCount;
            StartedAt = DateTime.UtcNow;
        }

        public int OperationCount { get; }
        public DateTime StartedAt { get; }
    }

    /// <summary>
    /// Event arguments for compensation completed events
    /// </summary>
    public class CompensationCompletedEventArgs : EventArgs
    {
        public CompensationCompletedEventArgs(CompensationResult result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public CompensationResult Result { get; }
    }

    /// <summary>
    /// Event arguments for individual operation compensation events
    /// </summary>
    public class OperationCompensatedEventArgs : EventArgs
    {
        public OperationCompensatedEventArgs(ICompensatableOperation operation, bool success, Exception? exception = null)
        {
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Success = success;
            Exception = exception;
            CompensatedAt = DateTime.UtcNow;
        }

        public ICompensatableOperation Operation { get; }
        public bool Success { get; }
        public Exception? Exception { get; }
        public DateTime CompensatedAt { get; }
    }
}