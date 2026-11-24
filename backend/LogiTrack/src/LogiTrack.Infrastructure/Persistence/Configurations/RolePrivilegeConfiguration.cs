using LogiTrack.Application.Common.Security;
using LogiTrack.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("role_permissions", "security");

        builder.HasKey(rolePrivilege => new { rolePrivilege.RoleCode, rolePrivilege.PermissionCode });

        builder.HasOne<Role>().WithMany().HasForeignKey(rolePrivilege => rolePrivilege.RoleCode).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Privilege>().WithMany().HasForeignKey(rolePrivilege => rolePrivilege.PermissionCode).OnDelete(DeleteBehavior.Cascade);

        // Permisos iniciales. Administrator tiene todo; Operator no crea usuarios; Driver solo consulta por ahora.
        // Para cambiarlos más adelante basta con modificar estas filas en una migración.
        var administrator = Permissions.All.Select(permission => new { RoleCode = "Administrator", PermissionCode = permission });
        var operatorRole = Permissions.All
            .Where(permission => permission != Permissions.UsersManage)
            .Select(permission => new { RoleCode = "Operator", PermissionCode = permission });
        var driver = new[] { Permissions.TrucksRead, Permissions.DriversRead, Permissions.AssignmentsRead }
            .Select(permission => new { RoleCode = "Driver", PermissionCode = permission });

        builder.HasData(administrator.Concat(operatorRole).Concat(driver).Cast<object>().ToArray());
    }
}
