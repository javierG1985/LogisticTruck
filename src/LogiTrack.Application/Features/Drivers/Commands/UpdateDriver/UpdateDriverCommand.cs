using LogiTrack.Application.Features.Drivers;
using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.UpdateDriver;

// Campos editables (la cédula y la fecha de nacimiento no cambian) más el id de la ruta y la versión de If-Match.
// La licencia se renueva con sus fechas nuevas. Devuelve la nueva versión para el ETag de la respuesta.
public sealed record UpdateDriverCommand(
    Guid Id,
    uint ExpectedVersion,
    string FirstName,
    string LastName,
    string Address,
    string Phone,
    string Email,
    string? ProfilePhotoUrl,
    LicenseCategory LicenseCategory,
    DateOnly LicenseIssuedOn,
    DateOnly LicenseExpiresOn) : ICommand<uint>, IDriverProfile;
