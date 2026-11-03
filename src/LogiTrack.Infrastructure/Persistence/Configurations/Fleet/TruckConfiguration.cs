using LogiTrack.Domain.Modules.Fleet.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations.Fleet;

public sealed class TruckConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.ToTable("trucks", "fleet", table =>
        {
            table.HasCheckConstraint("ck_trucks_capacity_kg_positive", "capacity_kg > 0");
            table.HasCheckConstraint("ck_trucks_volume_m3_positive", "volume_m3 > 0");
        });

        builder.HasKey(truck => truck.Id);

        // Los eventos son estado temporal del agregado: se despachan al guardar y nunca se persisten.
        builder.Ignore(truck => truck.DomainEvents);

        builder.Property(truck => truck.Plate)
            .HasConversion(plate => plate.Value, value => Plate.Create(value))
            .HasColumnType("varchar(6)")
            .IsRequired();

        builder.HasIndex(truck => truck.Plate).IsUnique();

        builder.Property(truck => truck.Brand).HasMaxLength(60).IsRequired();
        builder.Property(truck => truck.ModelYear);
        builder.Property(truck => truck.CapacityKg).HasPrecision(10, 2);
        builder.Property(truck => truck.VolumeM3).HasPrecision(8, 2);
        builder.Property(truck => truck.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(truck => truck.SoatExpiresOn);
        builder.Property(truck => truck.TechReviewExpiresOn);
        builder.Property(truck => truck.CreatedAt).HasColumnType("timestamptz");
        builder.Property(truck => truck.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(truck => truck.DeletedAt).HasColumnType("timestamptz");

        // Concurrencia optimista: Npgsql mapea esta propiedad a la columna de sistema xmin.
        builder.Property(truck => truck.Version).IsRowVersion();

        // Borrado lógico: los camiones dados de baja quedan fuera de todas las consultas.
        builder.HasQueryFilter(truck => truck.DeletedAt == null);
    }
}
