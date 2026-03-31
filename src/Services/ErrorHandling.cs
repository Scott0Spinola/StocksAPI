using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace src.Services;

public class ErrorHandling : IExceptionHandler
{

    private readonly ILogger<ErrorHandling> _logger;

    public ErrorHandling(ILogger<ErrorHandling> logger)
    {
        _logger = logger;
    }

   
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(exception, "Error occured for this traceId: " + traceId);

        var ( statusCode, title) = GetPromblemDetails(exception);

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

    public static (int statusCode, string title) GetPromblemDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided" + exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access" + exception.Message),
           _ =>(StatusCodes.Status500InternalServerError, "An unexpected error occured" + exception.Message),
        };
    }
}
