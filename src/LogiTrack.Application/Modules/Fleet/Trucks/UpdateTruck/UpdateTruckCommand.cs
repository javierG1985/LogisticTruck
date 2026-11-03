using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.UpdateTruck;

// Campos editables (sin placa, que es inmutable) más el id de la ruta y la versión de If-Match.
// Devuelve la nueva versión para que el controlador la envíe en el ETag de la respuesta.
public sealed record UpdateTruckCommand(
    Guid Id,
    uint ExpectedVersion,
    string Brand,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn) : ICommand<uint>;
