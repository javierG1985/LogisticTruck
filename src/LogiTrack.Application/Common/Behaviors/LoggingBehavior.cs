using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Common.Behaviors;

// LoggerMessage evita el coste de formatear mensajes cuando el nivel está deshabilitado.
public sealed partial class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        LogHandling(logger, messageName);

        try
        {
            var response = await next(message, cancellationToken);
            LogHandled(logger, messageName);
            return response;
        }
        catch (Exception exception)
        {
            LogFailed(logger, exception, messageName);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {MessageName}")]
    private static partial void LogHandling(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {MessageName}")]
    private static partial void LogHandled(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error handling {MessageName}")]
    private static partial void LogFailed(ILogger logger, Exception exception, string messageName);
}
