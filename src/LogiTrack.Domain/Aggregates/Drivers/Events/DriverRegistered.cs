using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers.Events;

public sealed record DriverRegistered(Guid DriverId, string DocumentNumber) : IDomainEvent;
