using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TaskMgr.Api.Middleware;

/// <summary>
/// Middleware for global error handling
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the ErrorHandlingMiddleware
    /// </summary>
    /// <param name="next">Next middleware in the pipeline</param>
    /// <param name="logger">Logger instance</param>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Task representing the middleware execution</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles exceptions and returns appropriate HTTP responses
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="exception">Exception that occurred</param>
    /// <returns>Task representing the exception handling</returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var problemDetails = exception switch
        {
            ValidationException validationException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path,
                Extensions = 
                {
                    ["errors"] = validationException.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                }
            },
            ArgumentNullException argumentNullException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = $"Required parameter '{argumentNullException.ParamName}' was null or empty.",
                Instance = context.Request.Path
            },
            ArgumentException argumentException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = argumentException.Message,
                Instance = context.Request.Path
            },
            InvalidOperationException invalidOperationException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = invalidOperationException.Message,
                Instance = context.Request.Path
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Forbidden,
                Detail = "You don't have permission to access this resource.",
                Instance = context.Request.Path
            },
            KeyNotFoundException notFoundException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = notFoundException.Message,
                Instance = context.Request.Path
            },
            TimeoutException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5",
                Title = "Request Timeout",
                Status = (int)HttpStatusCode.RequestTimeout,
                Detail = "The request timed out. Please try again later.",
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path
            }
        };

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension methods for registering the error handling middleware
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds the error handling middleware to the pipeline
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
