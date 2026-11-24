using LogiTrack.Domain.Aggregates.Users;

namespace LogiTrack.Application.Features.Users;

// Forma de un usuario hacia la API. Nunca incluye el hash de la contraseña.
public sealed record UserDetailsResponse(Guid Id, string Email, string Role)
{
    public static UserDetailsResponse From(User user) => new(user.Id, user.Email, user.RoleCode);
}
