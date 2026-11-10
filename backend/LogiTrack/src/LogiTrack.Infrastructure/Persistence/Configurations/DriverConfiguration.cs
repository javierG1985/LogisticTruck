using LogiTrack.Domain.Aggregates.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers", "fleet");

        builder.HasKey(driver => driver.Id);

        // Los eventos son estado temporal del agregado: se despachan al guardar y nunca se persisten.
        builder.Ignore(driver => driver.DomainEvents);

        builder.Property(driver => driver.DocumentNumber)
            .HasConversion(documentNumber => documentNumber.Value, value => DocumentNumber.Create(value))
            .HasColumnType("varchar(10)")
            .IsRequired();

        builder.HasIndex(driver => driver.DocumentNumber).IsUnique();

        builder.Property(driver => driver.FirstName).HasMaxLength(60).IsRequired();
        builder.Property(driver => driver.LastName).HasMaxLength(60).IsRequired();
        builder.Property(driver => driver.Address).HasMaxLength(200).IsRequired();
        builder.Property(driver => driver.Phone).HasMaxLength(20).IsRequired();
        builder.Property(driver => driver.Email).HasMaxLength(254).IsRequired();
        builder.Property(driver => driver.ProfilePhotoUrl).HasMaxLength(500);
        builder.Property(driver => driver.BirthDate);

        // Licencia como owned type: sus columnas viven en la misma fila del conductor.
        builder.OwnsOne(driver => driver.License, license =>
        {
            license.Property(l => l.Category)
                .HasConversion<string>()
                .HasColumnName("license_category")
                .HasMaxLength(2)
                .IsRequired();

            license.Property(l => l.IssuedOn).HasColumnName("license_issued_on");
            license.Property(l => l.ExpiresOn).HasColumnName("license_expires_on");
        });

        builder.Property(driver => driver.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(driver => driver.CreatedAt).HasColumnType("timestamptz");
        builder.Property(driver => driver.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(driver => driver.DeletedAt).HasColumnType("timestamptz");

        // Concurrencia optimista: Npgsql mapea esta propiedad a la columna de sistema xmin.
        builder.Property(driver => driver.Version).IsRowVersion();

        // Borrado lógico: los conductores dados de baja quedan fuera de todas las consultas.
        builder.HasQueryFilter(driver => driver.DeletedAt == null);
    }
}
