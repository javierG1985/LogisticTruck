using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.Features.Drivers;

// Datos de contacto y categoría de licencia que comparten crear y actualizar un conductor.
// Los comandos los cumplen con sus propiedades posicionales, y DriverProfileValidator revisa su forma una sola vez.
public interface IDriverProfile
{
    string FirstName { get; }

    string LastName { get; }

    string Address { get; }

    string Phone { get; }

    string Email { get; }

    string? ProfilePhotoUrl { get; }

    LicenseCategory LicenseCategory { get; }
}
