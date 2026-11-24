using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Api.Controllers.Drivers.Requests;

// Cuerpo de la renovación de licencia. El id viene en la ruta y la versión en If-Match.
public sealed record RenewDriverLicenseRequest(
    LicenseCategory LicenseCategory,
    DateOnly LicenseIssuedOn,
    DateOnly LicenseExpiresOn);
