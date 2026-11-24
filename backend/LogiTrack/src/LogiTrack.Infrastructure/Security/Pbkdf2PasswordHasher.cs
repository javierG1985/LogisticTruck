using System.Security.Cryptography;
using LogiTrack.Application.Common.Security;

namespace LogiTrack.Infrastructure.Security;

// Formato almacenado: pbkdf2-sha256$<iteraciones>$<sal base64>$<hash base64>.
// Las iteraciones quedan en el propio hash: si se suben en el futuro, los hashes antiguos siguen verificando.
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "pbkdf2-sha256";
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');

        if (parts.Length != 4 || parts[0] != Prefix
            || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Comparación en tiempo constante para no filtrar información por tiempo de respuesta.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
