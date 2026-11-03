using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Modules.Fleet.Trucks.Events;

public sealed record TruckStatusChanged(Guid TruckId, TruckStatus PreviousStatus, TruckStatus NewStatus) : IDomainEvent;
