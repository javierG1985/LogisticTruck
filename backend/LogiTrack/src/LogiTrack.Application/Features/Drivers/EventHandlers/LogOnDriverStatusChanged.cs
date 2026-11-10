using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Drivers.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Features.Drivers.EventHandlers;

// Registra el cambio de estado del conductor, igual que LogOnTruckStatusChanged.
public sealed partial class LogOnDriverStatusChanged(ILogger<LogOnDriverStatusChanged> logger)
    : INotificationHandler<DriverStatusChanged>
{
    public ValueTask Handle(DriverStatusChanged notification, CancellationToken cancellationToken)
    {
        LogStatusChanged(logger, notification.DriverId, notification.PreviousStatus, notification.NewStatus);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Driver {DriverId} changed status from {PreviousStatus} to {NewStatus}")]
    private static partial void LogStatusChanged(ILogger logger, Guid driverId, DriverStatus previousStatus, DriverStatus newStatus);
}
