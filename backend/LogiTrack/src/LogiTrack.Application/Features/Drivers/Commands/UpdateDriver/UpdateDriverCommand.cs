using LogiTrack.Application.Features.Drivers;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.UpdateDriver;

// Datos de contacto (la cédula, la fecha de nacimiento y la licencia no cambian aquí) más el id de la ruta y la versión de If-Match.
// Devuelve la nueva versión para el ETag de la respuesta.
public sealed record UpdateDriverCommand(
    Guid Id,
    uint ExpectedVersion,
    string FirstName,
    string LastName,
    string Address,
    string Phone,
    string Email,
    string? ProfilePhotoUrl) : ICommand<uint>, IDriverProfile;
