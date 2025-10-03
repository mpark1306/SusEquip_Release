using System;
using System.Collections.Generic;

namespace SusEquip.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when business rule operations fail.
    /// Used for business logic violations and domain rule enforcement.
    /// </summary>
    public class BusinessRuleException : SusEquipException
    {
        /// <summary>
        /// The business rule that was violated
        /// </summary>
        public string RuleName { get; }
        
        /// <summary>
        /// Additional rule context
        /// </summary>
        public Dictionary<string, object> RuleContext { get; }

        public BusinessRuleException(
            string ruleName,
            string message,
            string userMessage,
            Dictionary<string, object>? ruleContext = null,
            Exception? innerException = null)
            : base(
                errorCode: "BUSINESS_RULE_VIOLATION",
                message: message,
                userMessage: userMessage,
                severity: ErrorSeverity.Warning,
                innerException: innerException,
                errorContext: CreateContext(ruleName, ruleContext))
        {
            RuleName = ruleName;
            RuleContext = ruleContext ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates exception for status transition violations
        /// </summary>
        public static BusinessRuleException InvalidStatusTransition(
            string currentStatus,
            string targetStatus,
            string[] allowedTransitions)
        {
            var ruleContext = new Dictionary<string, object>
            {
                ["CurrentStatus"] = currentStatus,
                ["TargetStatus"] = targetStatus,
                ["AllowedTransitions"] = allowedTransitions
            };

            return new BusinessRuleException(
                ruleName: "StatusTransition",
                message: $"Invalid status transition from '{currentStatus}' to '{targetStatus}'",
                userMessage: $"Cannot change status from '{currentStatus}' to '{targetStatus}'. Allowed transitions: {string.Join(", ", allowedTransitions)}",
                ruleContext: ruleContext);
        }

        private static Dictionary<string, object> CreateContext(
            string ruleName,
            Dictionary<string, object>? ruleContext)
        {
            var context = new Dictionary<string, object>
            {
                ["RuleName"] = ruleName
            };
            
            if (ruleContext != null)
            {
                foreach (var kvp in ruleContext)
                {
                    context[kvp.Key] = kvp.Value;
                }
            }
            
            return context;
        }
    }
}