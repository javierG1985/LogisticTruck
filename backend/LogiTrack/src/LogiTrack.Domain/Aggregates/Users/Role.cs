namespace LogiTrack.Domain.Aggregates.Users;

// Rol de acceso. Es un catálogo de la base de datos (security.roles): sus permisos se cambian sin tocar el código.
public sealed class Role
{
    private Role()
    {
    }

    // Código estable que guarda el usuario y viaja en el token: "Administrator", "Operator" o "Driver".
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
}
