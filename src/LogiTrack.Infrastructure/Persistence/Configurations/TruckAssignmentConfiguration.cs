using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class TruckAssignmentConfiguration : IEntityTypeConfiguration<TruckAssignment>
{
    public void Configure(EntityTypeBuilder<TruckAssignment> builder)
    {
        builder.ToTable("truck_assignments", "fleet", table =>
        {
            // Una asignación liberada tiene motivo y fecha; una activa no tiene ninguno de los dos.
            table.HasCheckConstraint(
                "ck_truck_assignments_release_consistent",
                "(released_at IS NULL) = (release_reason IS NULL)");
        });

        builder.HasKey(assignment => assignment.Id);

        // Los eventos son estado temporal del agregado: se despachan al guardar y nunca se persisten.
        builder.Ignore(assignment => assignment.DomainEvents);

        builder.Property(assignment => assignment.TruckId).IsRequired();
        builder.Property(assignment => assignment.DriverId).IsRequired();
        builder.Property(assignment => assignment.AssignedAt).HasColumnType("timestamptz");
        builder.Property(assignment => assignment.ReleasedAt).HasColumnType("timestamptz");
        builder.Property(assignment => assignment.ReleaseReason).HasConversion<string>().HasMaxLength(30);

        // Integridad referencial hacia los camiones y conductores. No se borra en cascada: el borrado es lógico.
        builder.HasOne<Truck>()
            .WithMany()
            .HasForeignKey(assignment => assignment.TruckId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Driver>()
            .WithMany()
            .HasForeignKey(assignment => assignment.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices únicos parciales: un camión y un conductor tienen como mucho una asignación activa.
        // Son la última barrera frente a dos peticiones simultáneas; la comprobación previa del caso de uso es la primera.
        builder.HasIndex(assignment => assignment.TruckId)
            .IsUnique()
            .HasDatabaseName("ix_truck_assignments_active_truck_id")
            .HasFilter("released_at IS NULL");

        builder.HasIndex(assignment => assignment.DriverId)
            .IsUnique()
            .HasDatabaseName("ix_truck_assignments_active_driver_id")
            .HasFilter("released_at IS NULL");

        // Concurrencia optimista: Npgsql mapea esta propiedad a la columna de sistema xmin.
        builder.Property(assignment => assignment.Version).IsRowVersion();
    }
}
