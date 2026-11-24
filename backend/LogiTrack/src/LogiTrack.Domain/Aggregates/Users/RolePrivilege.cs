namespace LogiTrack.Domain.Aggregates.Users;

// Qué permisos tiene cada rol (security.role_permissions).
public sealed class RolePrivilege
{
    private RolePrivilege()
    {
    }

    public string RoleCode { get; private set; } = string.Empty;

    public string PermissionCode { get; private set; } = string.Empty;
}
