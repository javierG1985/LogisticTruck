namespace LogiTrack.Api.Requests.Fleet.Trucks;

// Cuerpo del PUT: reemplazo completo de los campos editables. El id viene en la ruta y la versión en If-Match.
public sealed record UpdateTruckRequest(
    string Brand,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn);
