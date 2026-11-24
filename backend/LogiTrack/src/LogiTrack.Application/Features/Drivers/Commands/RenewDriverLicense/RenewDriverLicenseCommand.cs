using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.RenewDriverLicense;

// Renovación de la licencia: sustituye la vigente por una expedida después de la actual y no en el futuro.
// Devuelve la nueva versión del conductor para el ETag de la respuesta.
public sealed record RenewDriverLicenseCommand(
    Guid Id,
    uint ExpectedVersion,
    LicenseCategory LicenseCategory,
    DateOnly LicenseIssuedOn,
    DateOnly LicenseExpiresOn) : ICommand<uint>;
