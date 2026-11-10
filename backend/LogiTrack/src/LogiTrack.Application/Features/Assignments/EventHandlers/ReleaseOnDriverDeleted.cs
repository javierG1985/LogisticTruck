using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers.Events;
using Mediator;

namespace LogiTrack.Application.Features.Assignments.EventHandlers;

public sealed class ReleaseOnDriverDeleted(IApplicationDbContext db, TimeProvider timeProvider)
    : INotificationHandler<DriverDeleted>
{
    public async ValueTask Handle(DriverDeleted notification, CancellationToken cancellationToken)
    {
        var driverId = notification.DriverId;

        await ActiveAssignments.ReleaseAsync(
            db,
            timeProvider,
            assignment => assignment.DriverId == driverId,
            AssignmentReleaseReason.DriverDeleted,
            cancellationToken);
    }
}
