using LogiTrack.Domain.Aggregates.Trucks;
using Mediator;

namespace LogiTrack.Application.Features.Trucks.Commands.UpdateTruck;

// Campos editables (sin placa, que es inmutable) más el id de la ruta y la versión de If-Match.
// Devuelve la nueva versión para que el controlador la envíe en el ETag de la respuesta.
public sealed record UpdateTruckCommand(
    Guid Id,
    uint ExpectedVersion,
    string Brand,
    TruckType TruckType,
    int ModelYear,
    decimal CapacityKg,
    decimal VolumeM3,
    DateOnly SoatExpiresOn,
    DateOnly TechReviewExpiresOn) : ICommand<uint>;
