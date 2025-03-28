using Microsoft.Extensions.Logging;

namespace Poll.N.Quiz.API.Shared;

public partial class GlobalLogger
{
    [LoggerMessage(1, LogLevel.Error, "{Message}, CorrelationId: {CorrelationId}", EventName = "Exception")]
    public static partial void LogException(ILogger logger, string message, string correlationId, Exception exception);

    [LoggerMessage(2, LogLevel.Warning, "{Message}, ServiceName: {ServiceName}, EnvironmentName: {EnvironmentName}", EventName = "Warning")]
    public static partial void LogWarning(ILogger logger, string message, string serviceName, string environmentName);
}
