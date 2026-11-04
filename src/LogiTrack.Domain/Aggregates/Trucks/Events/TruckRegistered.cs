using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Trucks.Events;

public sealed record TruckRegistered(Guid TruckId, string Plate) : IDomainEvent;
