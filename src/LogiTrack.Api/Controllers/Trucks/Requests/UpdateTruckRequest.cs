using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Api.Controllers.Trucks.Requests;

// Cuerpo del PUT: reemplazo completo de los campos editables. El id viene en la ruta y la versión en If-Match.
public sealed record UpdateTruckRequest(
    string Brand,
    TruckType TruckType,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn);
