using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Assignments.Events;

public sealed record AssignmentReleased(
    Guid AssignmentId,
    Guid TruckId,
    Guid DriverId,
    AssignmentReleaseReason Reason) : IDomainEvent;
