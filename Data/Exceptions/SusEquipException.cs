using System;
using System.Collections.Generic;

namespace SusEquip.Data.Exceptions
{
    /// <summary>
    /// Base exception class for all SusEquip domain-specific exceptions.
    /// Provides common functionality and structure for error handling.
    /// </summary>
    public abstract class SusEquipException : Exception
    {
        /// <summary>
        /// Unique error code identifying the specific type of error
        /// </summary>
        public string ErrorCode { get; }
        
        /// <summary>
        /// User-friendly error message that can be displayed to end users
        /// </summary>
        public string UserMessage { get; }
        
        /// <summary>
        /// Additional context data related to the error
        /// </summary>
        public Dictionary<string, object> ErrorContext { get; }
        
        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Severity level of the error
        /// </summary>
        public ErrorSeverity Severity { get; }

        protected SusEquipException(
            string errorCode,
            string message,
            string userMessage,
            ErrorSeverity severity = ErrorSeverity.Error,
            Exception? innerException = null,
            Dictionary<string, object>? errorContext = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            UserMessage = userMessage ?? throw new ArgumentNullException(nameof(userMessage));
            Severity = severity;
            ErrorContext = errorContext ?? new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
            
            // Add common context
            ErrorContext["Timestamp"] = Timestamp;
            ErrorContext["ErrorCode"] = ErrorCode;
            ErrorContext["Severity"] = Severity.ToString();
        }

        /// <summary>
        /// Adds context information to the error for debugging purposes
        /// </summary>
        public SusEquipException AddContext(string key, object value)
        {
            ErrorContext[key] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple context entries at once
        /// </summary>
        public SusEquipException AddContext(Dictionary<string, object> context)
        {
            if (context != null)
            {
                foreach (var kvp in context)
                {
                    ErrorContext[kvp.Key] = kvp.Value;
                }
            }
            return this;
        }

        /// <summary>
        /// Gets a formatted error message with context for logging
        /// </summary>
        public string GetDetailedMessage()
        {
            var details = new List<string>
            {
                $"Error Code: {ErrorCode}",
                $"Message: {Message}",
                $"User Message: {UserMessage}",
                $"Severity: {Severity}",
                $"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC"
            };

            if (ErrorContext.Count > 0)
            {
                details.Add("Context:");
                foreach (var kvp in ErrorContext)
                {
                    if (kvp.Key != "Timestamp" && kvp.Key != "ErrorCode" && kvp.Key != "Severity")
                    {
                        details.Add($"  {kvp.Key}: {kvp.Value}");
                    }
                }
            }

            return string.Join(Environment.NewLine, details);
        }
    }

    /// <summary>
    /// Represents the severity level of an error
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Informational message, not an error
        /// </summary>
        Info = 0,
        
        /// <summary>
        /// Warning that doesn't prevent operation
        /// </summary>
        Warning = 1,
        
        /// <summary>
        /// Error that prevents normal operation
        /// </summary>
        Error = 2,
        
        /// <summary>
        /// Critical error that may affect system stability
        /// </summary>
        Critical = 3,
        
        /// <summary>
        /// Fatal error that requires immediate attention
        /// </summary>
        Fatal = 4
    }
}