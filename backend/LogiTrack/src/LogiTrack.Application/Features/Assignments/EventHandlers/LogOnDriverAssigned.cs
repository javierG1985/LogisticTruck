using LogiTrack.Domain.Aggregates.Assignments.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Features.Assignments.EventHandlers;

// Registra la nueva asignación. Mediator exige un handler por cada notificación.
public sealed partial class LogOnDriverAssigned(ILogger<LogOnDriverAssigned> logger)
    : INotificationHandler<DriverAssigned>
{
    public ValueTask Handle(DriverAssigned notification, CancellationToken cancellationToken)
    {
        LogAssigned(logger, notification.AssignmentId, notification.DriverId, notification.TruckId);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Assignment {AssignmentId}: driver {DriverId} assigned to truck {TruckId}")]
    private static partial void LogAssigned(ILogger logger, Guid assignmentId, Guid driverId, Guid truckId);
}
