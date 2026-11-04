using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Application.Features.Trucks.Queries.ListTrucks;

public sealed record TruckListItem(
    Guid Id,
    string Plate,
    string Brand,
    int ModelYear,
    decimal CapacityKg,
    TruckStatus Status);
