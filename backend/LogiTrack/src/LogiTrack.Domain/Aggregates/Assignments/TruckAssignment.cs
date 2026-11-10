using LogiTrack.Domain.Aggregates.Assignments.Events;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Assignments;

// Vínculo entre un camión y un conductor. Solo referencia a ambos por id: las reglas que los combinan viven en TruckAssignmentPolicy.
// Una asignación activa tiene ReleasedAt nulo; las liberadas se conservan como historial.
public sealed class TruckAssignment : AggregateRoot
{
    // Constructor sin parámetros para que EF Core materialice el agregado desde la base de datos.
    private TruckAssignment() : base(Guid.Empty)
    {
    }

    private TruckAssignment(Guid id) : base(id)
    {
    }

    public Guid TruckId { get; private set; }

    public Guid DriverId { get; private set; }

    public DateTime AssignedAt { get; private set; }

    public DateTime? ReleasedAt { get; private set; }

    public AssignmentReleaseReason? ReleaseReason { get; private set; }

    public bool IsActive => ReleasedAt is null;

    // Versión para concurrencia optimista: EF Core la mapea a la columna xmin de PostgreSQL.
    public uint Version { get; private set; }

    // La aplicación debe llamar antes a TruckAssignmentPolicy.EnsureCanAssign con el camión y el conductor.
    public static TruckAssignment Start(Guid truckId, Guid driverId, DateTime utcNow)
    {
        var assignment = new TruckAssignment(Guid.CreateVersion7())
        {
            TruckId = truckId,
            DriverId = driverId,
            AssignedAt = utcNow,
        };

        assignment.Raise(new DriverAssigned(assignment.Id, truckId, driverId));

        return assignment;
    }

    public void Release(AssignmentReleaseReason reason, DateTime utcNow)
    {
        if (!IsActive)
        {
            throw new DomainException("La asignación ya fue liberada.");
        }

        ReleasedAt = utcNow;
        ReleaseReason = reason;

        Raise(new AssignmentReleased(Id, TruckId, DriverId, reason));
    }
}
