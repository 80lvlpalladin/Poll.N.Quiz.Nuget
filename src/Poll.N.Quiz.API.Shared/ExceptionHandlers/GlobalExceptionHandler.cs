using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Poll.N.Quiz.API.Shared.ExceptionHandlers;

public class GlobalExceptionHandler(string? environment, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = exception.Source ?? string.Empty;
        GlobalLogger.HandledException(logger, exception.Message, correlationId, exception);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = IsDevelopment() ? exception.Message : "Internal server error"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private bool IsDevelopment() =>
        environment is null ||
        string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);
}

public partial class GlobalLogger
{
    /// <summary>
    /// Logs a handled exception.
    /// </summary>
    [LoggerMessage(1, LogLevel.Error, "{Message}, CorrelationId: {CorrelationId}", EventName = "HandledException")]
    public static partial void HandledException(ILogger logger, string message, string correlationId, Exception exception);
}
