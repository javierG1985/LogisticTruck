using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Assignments;

// Regla que combina camión y conductor. Es la única pieza del dominio que conoce a ambos agregados
// (excepción documentada en ArchitectureTests): ni Truck ni Driver se referencian entre sí.
public static class TruckAssignmentPolicy
{
    public static void EnsureCanAssign(Truck truck, Driver driver, DateOnly today)
    {
        if (truck.DeletedAt is not null)
        {
            throw new DomainException("No se puede asignar un camión dado de baja.");
        }

        if (truck.Status != TruckStatus.Available)
        {
            throw new DomainException("Solo se puede asignar un camión disponible.");
        }

        if (driver.DeletedAt is not null)
        {
            throw new DomainException("No se puede asignar un conductor dado de baja.");
        }

        if (driver.Status != DriverStatus.Active)
        {
            throw new DomainException("Solo se puede asignar un conductor activo.");
        }

        if (!driver.License.IsValidOn(today))
        {
            throw new DomainException("La licencia del conductor no está vigente.");
        }

        if (!driver.License.Allows(RequiredLicenseFor(truck.TruckType)))
        {
            throw new DomainException("La licencia del conductor no alcanza la categoría que exige el camión.");
        }
    }

    // Categoría mínima de licencia por tipo de camión. Una categoría superior habilita las inferiores.
    public static LicenseCategory RequiredLicenseFor(TruckType truckType) => truckType switch
    {
        TruckType.Light => LicenseCategory.C1,
        TruckType.Rigid => LicenseCategory.C2,
        TruckType.Articulated => LicenseCategory.C3,
        _ => throw new ArgumentOutOfRangeException(nameof(truckType), truckType, "Tipo de camión desconocido."),
    };
}
