using LogiTrack.Domain.Modules.Fleet.Trucks.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Modules.Fleet.Trucks.EventHandlers;

// Mediator exige un handler para cada evento de dominio. Este registra el alta, igual que LogOnTruckStatusChanged.
public sealed partial class LogOnTruckRegistered(ILogger<LogOnTruckRegistered> logger)
    : INotificationHandler<TruckRegistered>
{
    public ValueTask Handle(TruckRegistered notification, CancellationToken cancellationToken)
    {
        LogRegistered(logger, notification.TruckId, notification.Plate);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Truck {TruckId} registered with plate {Plate}")]
    private static partial void LogRegistered(ILogger logger, Guid truckId, string plate);
}
