using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Api.Controllers.Drivers.Requests;

// Cuerpo del PUT: reemplazo completo de los campos editables. El id viene en la ruta y la versión en If-Match.
public sealed record UpdateDriverRequest(
    string FirstName,
    string LastName,
    string Address,
    string Phone,
    string Email,
    string? ProfilePhotoUrl,
    LicenseCategory LicenseCategory,
    DateOnly LicenseIssuedOn,
    DateOnly LicenseExpiresOn);
