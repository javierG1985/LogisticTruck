using LogiTrack.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Seguridad y acceso no son parte de la flota: van en su propio esquema.
        builder.ToTable("users", "security", table =>
        {
            table.HasCheckConstraint("ck_users_email_lowercase", "email = lower(email)");
        });

        builder.HasKey(user => user.Id);

        // Los eventos son estado temporal del agregado: nunca se persisten.
        builder.Ignore(user => user.DomainEvents);

        builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();

        // Hash PBKDF2 en formato "v1.iteraciones.sal.hash": cabe de sobra en 300 caracteres.
        builder.Property(user => user.PasswordHash).HasMaxLength(300).IsRequired();
        // El rol es una clave foránea a security.roles: no se puede guardar un rol que no exista.
        builder.Property(user => user.RoleCode).HasMaxLength(60).IsRequired();
        builder.HasOne<Role>().WithMany().HasForeignKey(user => user.RoleCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(user => user.CreatedAt).HasColumnType("timestamptz");

        // Concurrencia optimista: Npgsql mapea esta propiedad a la columna de sistema xmin.
        builder.Property(user => user.Version).IsRowVersion();
    }
}
