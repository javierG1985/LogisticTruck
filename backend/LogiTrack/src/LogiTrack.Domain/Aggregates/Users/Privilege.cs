namespace LogiTrack.Domain.Aggregates.Users;

// Permiso de acceso (security.permissions). El código lo usan las políticas de la API, por ejemplo "trucks.write".
public sealed class Privilege
{
    private Privilege()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;
}
