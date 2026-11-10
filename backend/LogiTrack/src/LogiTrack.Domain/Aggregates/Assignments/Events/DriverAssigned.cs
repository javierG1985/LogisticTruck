using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Assignments.Events;

public sealed record DriverAssigned(Guid AssignmentId, Guid TruckId, Guid DriverId) : IDomainEvent;
