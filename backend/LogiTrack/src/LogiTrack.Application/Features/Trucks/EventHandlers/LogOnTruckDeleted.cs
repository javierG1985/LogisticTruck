using LogiTrack.Domain.Aggregates.Trucks.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Features.Trucks.EventHandlers;

// Registra la baja del camión. Mediator exige un handler por cada notificación.
public sealed partial class LogOnTruckDeleted(ILogger<LogOnTruckDeleted> logger)
    : INotificationHandler<TruckDeleted>
{
    public ValueTask Handle(TruckDeleted notification, CancellationToken cancellationToken)
    {
        LogDeleted(logger, notification.TruckId);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Truck {TruckId} was deleted")]
    private static partial void LogDeleted(ILogger logger, Guid truckId);
}
