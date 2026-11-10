using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Trucks.Events;

public sealed record TruckDeleted(Guid TruckId) : IDomainEvent;
