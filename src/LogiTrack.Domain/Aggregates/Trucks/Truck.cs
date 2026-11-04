using LogiTrack.Domain.Aggregates.Trucks.Events;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Trucks;

public sealed class Truck : AggregateRoot
{
    // Constructor sin parámetros para que EF Core materialice el agregado desde la base de datos.
    private Truck() : base(Guid.Empty)
    {
    }

    private Truck(Guid id) : base(id)
    {
    }

    public Plate Plate { get; private set; } = null!;

    public string Brand { get; private set; } = string.Empty;

    public int ModelYear { get; private set; }

    public decimal CapacityKg { get; private set; }

    public decimal VolumeM3 { get; private set; }

    public TruckStatus Status { get; private set; }

    public DateOnly SoatExpiresOn { get; private set; }

    public DateOnly TechReviewExpiresOn { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    // Versión para concurrencia optimista: EF Core la mapea a la columna xmin de PostgreSQL.
    public uint Version { get; private set; }

    public static Truck Register(
        Plate plate,
        string brand,
        int modelYear,
        decimal capacityKg,
        decimal volumeM3,
        DateOnly soatExpiresOn,
        DateOnly techReviewExpiresOn,
        DateTime utcNow)
    {
        ValidateCapacities(capacityKg, volumeM3);

        var truck = new Truck(Guid.CreateVersion7())
        {
            Plate = plate,
            Status = TruckStatus.Available,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        truck.ApplyDetails(brand, modelYear, capacityKg, volumeM3, soatExpiresOn, techReviewExpiresOn);
        truck.Raise(new TruckRegistered(truck.Id, plate.Value));

        return truck;
    }

    public void Update(
        string brand,
        int modelYear,
        decimal capacityKg,
        decimal volumeM3,
        DateOnly soatExpiresOn,
        DateOnly techReviewExpiresOn,
        DateTime utcNow)
    {
        EnsureNotDeleted();
        ValidateCapacities(capacityKg, volumeM3);

        ApplyDetails(brand, modelYear, capacityKg, volumeM3, soatExpiresOn, techReviewExpiresOn);
        UpdatedAt = utcNow;
    }

    public void ChangeStatus(TruckStatus newStatus, DateTime utcNow)
    {
        EnsureNotDeleted();

        if (newStatus == Status)
        {
            return;
        }

        if (Status == TruckStatus.Maintenance && newStatus == TruckStatus.InRoute)
        {
            throw new DomainException("Un camión en mantenimiento no puede pasar a ruta.");
        }

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = utcNow;

        Raise(new TruckStatusChanged(Id, previousStatus, newStatus));
    }

    public void Delete(DateTime utcNow)
    {
        // Idempotente en su efecto: borrar un camión ya borrado no cambia nada.
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = utcNow;
        UpdatedAt = utcNow;
    }

    private void ApplyDetails(
        string brand,
        int modelYear,
        decimal capacityKg,
        decimal volumeM3,
        DateOnly soatExpiresOn,
        DateOnly techReviewExpiresOn)
    {
        Brand = brand;
        ModelYear = modelYear;
        CapacityKg = capacityKg;
        VolumeM3 = volumeM3;
        SoatExpiresOn = soatExpiresOn;
        TechReviewExpiresOn = techReviewExpiresOn;
    }

    private void EnsureNotDeleted()
    {
        if (DeletedAt is not null)
        {
            throw new DomainException("Un camión dado de baja no se puede modificar.");
        }
    }

    private static void ValidateCapacities(decimal capacityKg, decimal volumeM3)
    {
        if (capacityKg <= 0)
        {
            throw new DomainException("La capacidad debe ser mayor que cero.");
        }

        if (volumeM3 <= 0)
        {
            throw new DomainException("El volumen debe ser mayor que cero.");
        }
    }
}
