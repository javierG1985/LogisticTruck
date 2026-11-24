using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Users;

// Usuario que inicia sesión. La contraseña llega ya como hash: el dominio nunca ve la contraseña en claro.
public sealed class User : AggregateRoot
{
    // Constructor sin parámetros para que EF Core materialice el agregado desde la base de datos.
    private User() : base(Guid.Empty)
    {
    }

    private User(Guid id) : base(id)
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    // Código del rol (security.roles). Que exista se comprueba en Application: los roles viven en la base de datos.
    public string RoleCode { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    // Versión para concurrencia optimista: EF Core la mapea a la columna xmin de PostgreSQL.
    public uint Version { get; private set; }

    public static User Create(string email, string passwordHash, string roleCode, DateTime utcNow)
    {
        var normalized = NormalizeEmail(email);

        if (normalized.Length == 0 || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("El correo del usuario no es válido.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("La contraseña del usuario es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(roleCode))
        {
            throw new DomainException("El rol del usuario es obligatorio.");
        }

        return new User(Guid.CreateVersion7())
        {
            Email = normalized,
            PasswordHash = passwordHash,
            RoleCode = roleCode.Trim(),
            CreatedAt = utcNow,
        };
    }

    // El correo es el identificador de inicio de sesión: se compara sin espacios y sin distinguir mayúsculas.
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
