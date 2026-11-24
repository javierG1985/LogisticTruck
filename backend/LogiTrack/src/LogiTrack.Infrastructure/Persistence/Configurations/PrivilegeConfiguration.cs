using LogiTrack.Application.Common.Security;
using LogiTrack.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.ToTable("permissions", "security");

        builder.HasKey(permission => permission.Code);
        builder.Property(permission => permission.Code).HasMaxLength(60);
        builder.Property(permission => permission.Description).HasMaxLength(200).IsRequired();

        builder.HasData(
            new { Code = Permissions.TrucksRead, Description = "Consultar camiones" },
            new { Code = Permissions.TrucksWrite, Description = "Registrar, editar, cambiar estado y dar de baja camiones" },
            new { Code = Permissions.DriversRead, Description = "Consultar conductores" },
            new { Code = Permissions.DriversWrite, Description = "Registrar, editar, renovar licencia, cambiar estado y dar de baja conductores" },
            new { Code = Permissions.AssignmentsRead, Description = "Consultar asignaciones" },
            new { Code = Permissions.AssignmentsWrite, Description = "Asignar y liberar conductores de camiones" },
            new { Code = Permissions.UsersManage, Description = "Crear usuarios" });
    }
}
