namespace LogiTrack.Application.Common.Security;

// Emite el token de acceso. La implementación (JWT) vive en la capa Api, que es la que conoce la configuración de autenticación.
public interface ITokenIssuer
{
    IssuedToken Issue(Guid userId, string email, string role, IReadOnlyCollection<string> permissions);
}

public sealed record IssuedToken(string AccessToken, int ExpiresInSeconds);
