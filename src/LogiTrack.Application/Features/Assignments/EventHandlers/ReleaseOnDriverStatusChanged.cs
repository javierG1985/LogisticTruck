using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Drivers.Events;
using Mediator;

namespace LogiTrack.Application.Features.Assignments.EventHandlers;

// Un conductor de vacaciones, incapacidad o retirado no puede seguir con un camión asignado.
// Volver a estar activo no restaura la asignación: hay que asignarlo otra vez.
public sealed class ReleaseOnDriverStatusChanged(IApplicationDbContext db, TimeProvider timeProvider)
    : INotificationHandler<DriverStatusChanged>
{
    public async ValueTask Handle(DriverStatusChanged notification, CancellationToken cancellationToken)
    {
        if (notification.NewStatus is not (DriverStatus.OnLeave or DriverStatus.Inactive))
        {
            return;
        }

        var driverId = notification.DriverId;

        await ActiveAssignments.ReleaseAsync(
            db,
            timeProvider,
            assignment => assignment.DriverId == driverId,
            AssignmentReleaseReason.DriverStatusChanged,
            cancellationToken);
    }
}
