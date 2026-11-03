using LogiTrack.Domain.Modules.Fleet.Trucks;
using LogiTrack.Domain.Modules.Fleet.Trucks.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Modules.Fleet.Trucks.EventHandlers;

// Registra el cambio de estado: confirma que el despacho de eventos de dominio funciona tras guardar.
public sealed partial class LogOnTruckStatusChanged(ILogger<LogOnTruckStatusChanged> logger)
    : INotificationHandler<TruckStatusChanged>
{
    public ValueTask Handle(TruckStatusChanged notification, CancellationToken cancellationToken)
    {
        LogStatusChanged(logger, notification.TruckId, notification.PreviousStatus, notification.NewStatus);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Truck {TruckId} changed status from {PreviousStatus} to {NewStatus}")]
    private static partial void LogStatusChanged(ILogger logger, Guid truckId, TruckStatus previousStatus, TruckStatus newStatus);
}
