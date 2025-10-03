using System.Diagnostics;
using FluentAssertions;
using SusEquip.Data.Models;

namespace SusEquip.Tests.Infrastructure
{
    /// <summary>
    /// Utility methods for testing and assertions
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// Measures the execution time of an operation
        /// </summary>
        public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await operation();
            stopwatch.Stop();
            return (result, stopwatch.Elapsed);
        }

        /// <summary>
        /// Measures the execution time of an operation without return value
        /// </summary>
        public static async Task<TimeSpan> MeasureAsync(Func<Task> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            await operation();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Executes an operation multiple times concurrently and measures performance
        /// </summary>
        public static async Task<(List<T> Results, TimeSpan TotalDuration, TimeSpan AverageDuration)> 
            ExecuteConcurrentlyAsync<T>(Func<int, Task<T>> operation, int concurrency)
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = Enumerable.Range(0, concurrency)
                .Select(i => operation(i))
                .ToArray();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            return (results.ToList(), stopwatch.Elapsed, TimeSpan.FromTicks(stopwatch.ElapsedTicks / concurrency));
        }

        /// <summary>
        /// Failure simulator for testing error handling
        /// </summary>
        public class FailureSimulator
        {
            private readonly Random _random = new Random();
            private int _callCount = 0;

            public double FailureRate { get; set; } = 0.3; // 30% failure rate by default
            public int FailAfterCalls { get; set; } = -1; // Fail after specific number of calls
            public int ConsecutiveFailures { get; set; } = 1; // Number of consecutive failures
            private int _failureCount = 0;

            /// <summary>
            /// Simulates a potentially failing operation
            /// </summary>
            public async Task<T> SimulateAsync<T>(Func<Task<T>> operation, string errorMessage = "Simulated failure")
            {
                _callCount++;

                // Check if we should fail based on call count
                if (FailAfterCalls > 0 && _callCount >= FailAfterCalls)
                {
                    if (_failureCount < ConsecutiveFailures)
                    {
                        _failureCount++;
                        throw new InvalidOperationException($"{errorMessage} (Call #{_callCount})");
                    }
                    else
                    {
                        _failureCount = 0; // Reset after consecutive failures
                    }
                }

                // Check if we should fail based on failure rate
                if (_random.NextDouble() < FailureRate)
                {
                    throw new InvalidOperationException($"{errorMessage} (Random failure at call #{_callCount})");
                }

                return await operation();
            }

            /// <summary>
            /// Simulates a potentially failing operation for void Task operations
            /// </summary>
            public async Task SimulateAsync(Func<Task> operation, string errorMessage = "Simulated failure")
            {
                _callCount++;

                // Check if we should fail based on call count
                if (FailAfterCalls > 0 && _callCount >= FailAfterCalls)
                {
                    if (_failureCount < ConsecutiveFailures)
                    {
                        _failureCount++;
                        throw new InvalidOperationException($"{errorMessage} (Call #{_callCount})");
                    }
                    else
                    {
                        _failureCount = 0; // Reset after consecutive failures
                    }
                }

                // Check if we should fail based on failure rate
                if (_random.NextDouble() < FailureRate)
                {
                    throw new InvalidOperationException($"{errorMessage} (Random failure at call #{_callCount})");
                }

                await operation();
            }

            /// <summary>
            /// Resets the failure simulator state
            /// </summary>
            public void Reset()
            {
                _callCount = 0;
                _failureCount = 0;
            }
        }

        /// <summary>
        /// Waits for a condition to be true with timeout
        /// </summary>
        public static async Task<bool> WaitForConditionAsync(
            Func<bool> condition,
            TimeSpan timeout,
            TimeSpan? checkInterval = null)
        {
            var interval = checkInterval ?? TimeSpan.FromMilliseconds(100);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                if (condition())
                    return true;

                await Task.Delay(interval);
            }

            return false;
        }
    }

    /// <summary>
    /// Custom assertions for Phase 5 patterns
    /// </summary>
    public static class Phase5Assertions
    {
        /// <summary>
        /// Asserts that a validation result indicates success
        /// </summary>
        public static void ShouldIndicateSuccess(this List<ValidationIssue> validationIssues)
        {
            validationIssues.Should().BeEmpty("Validation should succeed with no issues");
        }

        /// <summary>
        /// Asserts that a validation result indicates failure
        /// </summary>
        public static void ShouldIndicateValidationFailure(this List<ValidationIssue> validationIssues)
        {
            validationIssues.Should().NotBeEmpty("Validation should fail with issues");
        }

        /// <summary>
        /// Asserts that a compensation result indicates success
        /// </summary>
        public static void ShouldIndicateCompensationSuccess(this CompensationResult result)
        {
            result.IsSuccess.Should().BeTrue("Compensation should succeed");
            result.Message.Should().NotBeNullOrEmpty("Compensation should have a success message");
        }

        /// <summary>
        /// Asserts that a compensation result indicates failure
        /// </summary>
        public static void ShouldIndicateCompensationFailure(this CompensationResult result)
        {
            result.IsSuccess.Should().BeFalse("Compensation should fail");
            result.Message.Should().NotBeNullOrEmpty("Compensation should have a failure message");
        }

        /// <summary>
        /// Asserts that a service result indicates success
        /// </summary>
        public static void ShouldIndicateSuccess<T>(this ServiceResult<T> result)
        {
            result.IsSuccess.Should().BeTrue(result.Message ?? "Operation should succeed");
            result.Data.Should().NotBeNull();
        }

        /// <summary>
        /// Asserts that a service result indicates validation failure
        /// </summary>
        public static void ShouldIndicateValidationFailure<T>(this ServiceResult<T> result)
        {
            result.IsSuccess.Should().BeFalse();
            result.ValidationIssues.Should().NotBeEmpty();
        }
    }

    // Stub classes to enable test compilation
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<ValidationIssue> ValidationIssues { get; set; } = new();
    }

    public class CompensationResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }

    public class CompensationOperationResult
    {
        public string OperationName { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public Exception? Exception { get; set; }
    }
}