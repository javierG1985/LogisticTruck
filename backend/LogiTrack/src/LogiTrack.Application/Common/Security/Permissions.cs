namespace LogiTrack.Application.Common.Security;

// Códigos de permiso que usa la API en sus políticas. Los roles y su asignación viven en la base de datos (security.role_permissions).
public static class Permissions
{
    // Nombre del claim de permiso dentro del token. Hay un claim por cada permiso del rol.
    public const string ClaimType = "permission";

    public const string TrucksRead = "trucks.read";
    public const string TrucksWrite = "trucks.write";
    public const string DriversRead = "drivers.read";
    public const string DriversWrite = "drivers.write";
    public const string AssignmentsRead = "assignments.read";
    public const string AssignmentsWrite = "assignments.write";
    public const string UsersManage = "users.manage";

    public static readonly string[] All =
    [
        TrucksRead,
        TrucksWrite,
        DriversRead,
        DriversWrite,
        AssignmentsRead,
        AssignmentsWrite,
        UsersManage,
    ];
}
