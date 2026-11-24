namespace LogiTrack.Application.Features.Drivers;

// Datos de contacto que comparten crear y actualizar un conductor. La licencia no está aquí: se renueva por su propio caso de uso.
public interface IDriverProfile
{
    string FirstName { get; }

    string LastName { get; }

    string Address { get; }

    string Phone { get; }

    string Email { get; }

    string? ProfilePhotoUrl { get; }
}
