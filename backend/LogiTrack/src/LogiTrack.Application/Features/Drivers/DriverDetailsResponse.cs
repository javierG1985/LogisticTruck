using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.Features.Drivers;

// Forma de un conductor completo. La comparten GetDriverById y CreateDriver, por eso vive en la carpeta del recurso.
public sealed record DriverDetailsResponse(
    Guid Id,
    string DocumentNumber,
    string FirstName,
    string LastName,
    string Address,
    string Phone,
    string Email,
    string? ProfilePhotoUrl,
    DateOnly BirthDate,
    LicenseCategory LicenseCategory,
    DateOnly LicenseIssuedOn,
    DateOnly LicenseExpiresOn,
    DriverStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    uint Version)
{
    public static DriverDetailsResponse From(Driver driver) =>
        new(
            driver.Id,
            driver.DocumentNumber.Value,
            driver.FirstName,
            driver.LastName,
            driver.Address,
            driver.Phone,
            driver.Email,
            driver.ProfilePhotoUrl,
            driver.BirthDate,
            driver.License.Category,
            driver.License.IssuedOn,
            driver.License.ExpiresOn,
            driver.Status,
            driver.CreatedAt,
            driver.UpdatedAt,
            driver.Version);
}
