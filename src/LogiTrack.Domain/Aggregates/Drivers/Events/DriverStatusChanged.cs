using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers.Events;

public sealed record DriverStatusChanged(Guid DriverId, DriverStatus PreviousStatus, DriverStatus NewStatus) : IDomainEvent;
