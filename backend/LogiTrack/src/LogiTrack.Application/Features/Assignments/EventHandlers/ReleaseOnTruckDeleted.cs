using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Trucks.Events;
using Mediator;

namespace LogiTrack.Application.Features.Assignments.EventHandlers;

public sealed class ReleaseOnTruckDeleted(IApplicationDbContext db, TimeProvider timeProvider)
    : INotificationHandler<TruckDeleted>
{
    public async ValueTask Handle(TruckDeleted notification, CancellationToken cancellationToken)
    {
        var truckId = notification.TruckId;

        await ActiveAssignments.ReleaseAsync(
            db,
            timeProvider,
            assignment => assignment.TruckId == truckId,
            AssignmentReleaseReason.TruckDeleted,
            cancellationToken);
    }
}
