using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Modules.Fleet.Trucks.Events;

public sealed record TruckRegistered(Guid TruckId, string Plate) : IDomainEvent;
