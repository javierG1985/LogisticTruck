using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.CreateTruck;

public sealed record CreateTruckCommand(
    string Plate,
    string Brand,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn) : ICommand<TruckDetailsResponse>;
