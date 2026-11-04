using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Application.Features.Trucks;

// Forma de un camión completo. La comparten GetTruckById y CreateTruck, por eso vive en la carpeta del recurso.
public sealed record TruckDetailsResponse(
    Guid Id,
    string Plate,
    string Brand,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    TruckStatus Status,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    uint Version)
{
    public static TruckDetailsResponse From(Truck truck) =>
        new(
            truck.Id,
            truck.Plate.Value,
            truck.Brand,
            truck.ModelYear,
            truck.CapacityKg,
            truck.VolumeM3,
            truck.Status,
            truck.SoatExpiresOn,
            truck.TechReviewExpiresOn,
            truck.CreatedAt,
            truck.UpdatedAt,
            truck.Version);
}
