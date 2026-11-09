using LogiTrack.Domain.Aggregates.Drivers.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Features.Drivers.EventHandlers;

// Mediator exige un handler para cada evento de dominio. Este registra el alta, igual que LogOnTruckRegistered.
public sealed partial class LogOnDriverRegistered(ILogger<LogOnDriverRegistered> logger)
    : INotificationHandler<DriverRegistered>
{
    public ValueTask Handle(DriverRegistered notification, CancellationToken cancellationToken)
    {
        LogRegistered(logger, notification.DriverId, notification.DocumentNumber);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Driver {DriverId} registered with document {DocumentNumber}")]
    private static partial void LogRegistered(ILogger logger, Guid driverId, string documentNumber);
}
