using LogiTrack.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "security");

        builder.HasKey(role => role.Code);
        builder.Property(role => role.Code).HasMaxLength(60);
        builder.Property(role => role.Name).HasMaxLength(60).IsRequired();

        // Roles iniciales. Se pueden añadir o renombrar más adelante mediante migraciones.
        builder.HasData(
            new { Code = "Administrator", Name = "Administrador" },
            new { Code = "Operator", Name = "Operador" },
            new { Code = "Driver", Name = "Conductor" });
    }
}
