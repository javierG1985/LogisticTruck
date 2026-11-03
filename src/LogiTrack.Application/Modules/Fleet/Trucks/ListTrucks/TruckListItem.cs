using LogiTrack.Domain.Modules.Fleet.Trucks;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ListTrucks;

public sealed record TruckListItem(
    Guid Id,
    string Plate,
    string Brand,
    int ModelYear,
    decimal CapacityKg,
    TruckStatus Status);
