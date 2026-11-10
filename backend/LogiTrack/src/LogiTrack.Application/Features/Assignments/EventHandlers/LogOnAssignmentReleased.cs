using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Assignments.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LogiTrack.Application.Features.Assignments.EventHandlers;

// Registra la liberación y su motivo. Mediator exige un handler por cada notificación.
public sealed partial class LogOnAssignmentReleased(ILogger<LogOnAssignmentReleased> logger)
    : INotificationHandler<AssignmentReleased>
{
    public ValueTask Handle(AssignmentReleased notification, CancellationToken cancellationToken)
    {
        LogReleased(logger, notification.AssignmentId, notification.DriverId, notification.TruckId, notification.Reason);
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Assignment {AssignmentId} released: driver {DriverId} from truck {TruckId} ({Reason})")]
    private static partial void LogReleased(ILogger logger, Guid assignmentId, Guid driverId, Guid truckId, AssignmentReleaseReason reason);
}
