using LogiTrack.Domain.Aggregates.Drivers.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Features.Drivers.EventHandlers;

// Registra la baja lógica del conductor.
public sealed partial class LogOnDriverDeleted(ILogger<LogOnDriverDeleted> logger)
    : INotificationHandler<DriverDeleted>
{
    public ValueTask Handle(DriverDeleted notification, CancellationToken cancellationToken)
    {
        LogDeleted(logger, notification.DriverId);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Driver {DriverId} deleted")]
    private static partial void LogDeleted(ILogger logger, Guid driverId);
}
