using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers.Events;

public sealed record DriverDeleted(Guid DriverId) : IDomainEvent;
