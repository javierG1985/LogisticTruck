using LogiTrack.Domain.Aggregates.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiTrack.Infrastructure.Persistence.Configurations;

public sealed class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable("catalog_items", "fleet");

        builder.HasKey(item => new { item.CatalogCode, item.Code });
        builder.Property(item => item.CatalogCode).HasMaxLength(60);
        builder.Property(item => item.Code).HasMaxLength(60);
        builder.Property(item => item.Label).HasMaxLength(120).IsRequired();

        // Datos iniciales. Los códigos son los nombres de los enums del dominio: si cambia un enum, hay que cambiar también esta lista (lo comprueba una prueba).
        builder.HasData(
            Item("truck_types", "Light", "Liviano", 1),
            Item("truck_types", "Rigid", "Rígido", 2),
            Item("truck_types", "Articulated", "Articulado", 3),
            Item("truck_statuses", "Available", "Disponible", 1),
            Item("truck_statuses", "InRoute", "En ruta", 2),
            Item("truck_statuses", "Maintenance", "En mantenimiento", 3),
            Item("truck_statuses", "Inactive", "Inactivo", 4),
            Item("driver_statuses", "Active", "Activo", 1),
            Item("driver_statuses", "OnLeave", "Vacaciones o incapacidad", 2),
            Item("driver_statuses", "Inactive", "Retirado", 3),
            Item("license_categories", "C1", "C1", 1),
            Item("license_categories", "C2", "C2", 2),
            Item("license_categories", "C3", "C3", 3),
            Item("license_statuses", "Valid", "Vigente", 1),
            Item("license_statuses", "ExpiringSoon", "Por vencer", 2),
            Item("license_statuses", "Expired", "Vencida", 3),
            Item("assignment_release_reasons", "Manual", "Liberación manual", 1),
            Item("assignment_release_reasons", "TruckStatusChanged", "Cambio de estado del camión", 2),
            Item("assignment_release_reasons", "TruckDeleted", "Camión dado de baja", 3),
            Item("assignment_release_reasons", "DriverStatusChanged", "Cambio de estado del conductor", 4),
            Item("assignment_release_reasons", "DriverDeleted", "Conductor dado de baja", 5));
    }

    private static object Item(string catalog, string code, string label, int sortOrder) =>
        new { CatalogCode = catalog, Code = code, Label = label, SortOrder = sortOrder };
}
