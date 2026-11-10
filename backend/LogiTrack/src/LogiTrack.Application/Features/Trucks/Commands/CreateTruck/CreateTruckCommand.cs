using LogiTrack.Domain.Aggregates.Trucks;
using Mediator;

namespace LogiTrack.Application.Features.Trucks.Commands.CreateTruck;

public sealed record CreateTruckCommand(
    string Plate,
    string Brand,
    TruckType TruckType,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn) : ICommand<TruckDetailsResponse>;
