using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Trucks;
using LogiTrack.Domain.Aggregates.Trucks.Events;
using Mediator;

namespace LogiTrack.Application.Features.Assignments.EventHandlers;

// Un camión en mantenimiento o fuera de servicio no puede seguir con conductor asignado.
// Pasar a ruta o volver a disponible no libera la asignación.
public sealed class ReleaseOnTruckStatusChanged(IApplicationDbContext db, TimeProvider timeProvider)
    : INotificationHandler<TruckStatusChanged>
{
    public async ValueTask Handle(TruckStatusChanged notification, CancellationToken cancellationToken)
    {
        if (notification.NewStatus is not (TruckStatus.Maintenance or TruckStatus.Inactive))
        {
            return;
        }

        var truckId = notification.TruckId;

        await ActiveAssignments.ReleaseAsync(
            db,
            timeProvider,
            assignment => assignment.TruckId == truckId,
            AssignmentReleaseReason.TruckStatusChanged,
            cancellationToken);
    }
}
