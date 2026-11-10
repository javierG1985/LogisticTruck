namespace LogiTrack.Domain.Aggregates.Assignments;

// Por qué terminó una asignación. Las razones automáticas las fijan los handlers de eventos (paso 6).
public enum AssignmentReleaseReason
{
    Manual,
    TruckStatusChanged,
    TruckDeleted,
    DriverStatusChanged,
    DriverDeleted,
}
