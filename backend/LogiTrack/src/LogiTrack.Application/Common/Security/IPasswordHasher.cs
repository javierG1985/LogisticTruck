namespace LogiTrack.Application.Common.Security;

// Hash de contraseñas. La implementación vive en Infrastructure; Application solo conoce el contrato.
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
