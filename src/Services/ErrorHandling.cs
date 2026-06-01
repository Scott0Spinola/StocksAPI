using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace src.Services;

/// <summary>
/// Centralized exception handler that converts unhandled exceptions into RFC 7807 Problem Details responses.
/// </summary>
public class ErrorHandling : IExceptionHandler
{

    private readonly ILogger<ErrorHandling> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ErrorHandling"/>.
    /// </summary>
    /// <param name="logger">Logger used to record exceptions and correlation identifiers.</param>
    public ErrorHandling(ILogger<ErrorHandling> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts to handle an exception by logging it and returning a Problem Details response.
    /// </summary>
    /// <param name="httpContext">Current HTTP request context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">Cancellation token (not currently used).</param>
    /// <returns><c>true</c> to indicate the exception was handled.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Prefer Activity Id when available so distributed traces correlate correctly.
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(exception, "Error occured for this traceId: " + traceId);

        var ( statusCode, title) = GetPromblemDetails(exception);

        // Emit a Problem Details payload with traceId + error message for debugging.
        await Results.Problem(
            title: title,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>()
            {
                {"traceId", traceId},
                {"erro", exception.Message}
            }
        ).ExecuteAsync(httpContext);

        return true;
    }

    /// <summary>
    /// Maps an exception to a problem details HTTP status code and title.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A tuple containing the HTTP status code and title.</returns>
    public static (int statusCode, string title) GetPromblemDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, $"Invalid argument provided: {exception.Message}"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, $"Unauthorized access: {exception.Message}"),
            HttpRequestException => (StatusCodes.Status502BadGateway, $"Upstream request failed: {exception.Message}"),
           _ => (StatusCodes.Status500InternalServerError, "An unexpected error occured."),
        };
    }
}
