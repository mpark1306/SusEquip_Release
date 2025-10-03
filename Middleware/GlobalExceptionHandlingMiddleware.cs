using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Exceptions;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SusEquip.Middleware
{
    /// <summary>
    /// Middleware for global exception handling and structured error responses.
    /// Catches unhandled exceptions and converts them to appropriate HTTP responses.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly bool _includeStackTrace;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _includeStackTrace = environment.IsDevelopment();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorResponse = CreateErrorResponse(exception);
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            // Log the exception with appropriate level
            LogException(exception, context);

            // Set response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.StatusCode;

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponse CreateErrorResponse(Exception exception)
        {
            return exception switch
            {
                SusEquipException susEquipEx => new ErrorResponse
                {
                    StatusCode = GetStatusCodeForSusEquipException(susEquipEx),
                    ErrorCode = susEquipEx.ErrorCode,
                    Message = susEquipEx.UserMessage,
                    DetailedMessage = _includeStackTrace ? susEquipEx.GetDetailedMessage() : null,
                    Timestamp = susEquipEx.Timestamp,
                    Context = _includeStackTrace ? susEquipEx.ErrorContext : null,
                    StackTrace = _includeStackTrace ? susEquipEx.StackTrace : null
                },

                ArgumentNullException argNullEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "ARGUMENT_NULL",
                    Message = "A required parameter was not provided.",
                    DetailedMessage = _includeStackTrace ? argNullEx.Message : null,
                    StackTrace = _includeStackTrace ? argNullEx.StackTrace : null
                },

                ArgumentException argEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "INVALID_ARGUMENT",
                    Message = "One or more parameters are invalid.",
                    DetailedMessage = _includeStackTrace ? argEx.Message : null,
                    StackTrace = _includeStackTrace ? argEx.StackTrace : null
                },

                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    ErrorCode = "UNAUTHORIZED_ACCESS",
                    Message = "You do not have permission to perform this action."
                },

                TimeoutException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.RequestTimeout,
                    ErrorCode = "OPERATION_TIMEOUT",
                    Message = "The operation timed out. Please try again."
                },

                NotImplementedException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotImplemented,
                    ErrorCode = "NOT_IMPLEMENTED",
                    Message = "This feature is not yet implemented."
                },

                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "An unexpected error occurred. Please try again later.",
                    DetailedMessage = _includeStackTrace ? exception.Message : null,
                    StackTrace = _includeStackTrace ? exception.StackTrace : null
                }
            };
        }

        private static int GetStatusCodeForSusEquipException(SusEquipException exception)
        {
            return exception switch
            {
                EquipmentNotFoundException => (int)HttpStatusCode.NotFound,
                EquipmentValidationException => (int)HttpStatusCode.BadRequest,
                DuplicateSerialNumberException => (int)HttpStatusCode.Conflict,
                BusinessRuleException => (int)HttpStatusCode.UnprocessableEntity,
                DatabaseOperationException => (int)HttpStatusCode.InternalServerError,
                _ => exception.Severity switch
                {
                    ErrorSeverity.Warning => (int)HttpStatusCode.BadRequest,
                    ErrorSeverity.Error => (int)HttpStatusCode.InternalServerError,
                    ErrorSeverity.Critical => (int)HttpStatusCode.InternalServerError,
                    ErrorSeverity.Fatal => (int)HttpStatusCode.InternalServerError,
                    _ => (int)HttpStatusCode.InternalServerError
                }
            };
        }

        private void LogException(Exception exception, HttpContext context)
        {
            var requestPath = context.Request.Path.Value ?? "Unknown";
            var requestMethod = context.Request.Method;
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
            
            var logContext = new
            {
                RequestPath = requestPath,
                RequestMethod = requestMethod,
                UserAgent = userAgent,
                RequestId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case SusEquipException susEquipEx:
                    LogSusEquipException(susEquipEx, logContext);
                    break;

                case ArgumentNullException:
                case ArgumentException:
                    _logger.LogWarning(exception,
                        "Argument validation failed for {RequestMethod} {RequestPath}. Context: {@LogContext}",
                        requestMethod, requestPath, logContext);
                    break;

                case UnauthorizedAccessException:
                    _logger.LogWarning(exception,
                        "Unauthorized access attempt for {RequestMethod} {RequestPath}. Context: {@LogContext}",
                        requestMethod, requestPath, logContext);
                    break;

                case TimeoutException:
                    _logger.LogWarning(exception,
                        "Operation timeout for {RequestMethod} {RequestPath}. Context: {@LogContext}",
                        requestMethod, requestPath, logContext);
                    break;

                default:
                    _logger.LogError(exception,
                        "Unhandled exception occurred for {RequestMethod} {RequestPath}. Context: {@LogContext}",
                        requestMethod, requestPath, logContext);
                    break;
            }
        }

        private void LogSusEquipException(SusEquipException exception, object logContext)
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
                "SusEquip domain exception occurred: {ErrorCode} - {Message}. Context: {@ExceptionContext}, Request: {@LogContext}",
                exception.ErrorCode, exception.Message, exception.ErrorContext, logContext);
        }
    }

    /// <summary>
    /// Structured error response model for consistent API responses
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? DetailedMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Context { get; set; }
        public string? StackTrace { get; set; }
    }

    /// <summary>
    /// Extension methods for registering the global exception handling middleware
    /// </summary>
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds global exception handling middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}