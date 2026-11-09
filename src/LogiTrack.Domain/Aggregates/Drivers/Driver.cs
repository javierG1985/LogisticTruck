using LogiTrack.Domain.Aggregates.Drivers.Events;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers;

public sealed class Driver : AggregateRoot
{
    // Constructor sin parámetros para que EF Core materialice el agregado desde la base de datos.
    private Driver() : base(Guid.Empty)
    {
    }

    private Driver(Guid id) : base(id)
    {
    }

    public DocumentNumber DocumentNumber { get; private set; } = null!;

    public string FullName { get; private set; } = string.Empty;

    public DateOnly BirthDate { get; private set; }

    public DriverLicense License { get; private set; } = null!;

    public DriverStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    // Versión para concurrencia optimista: EF Core la mapea a la columna xmin de PostgreSQL.
    public uint Version { get; private set; }

    public static Driver Register(
        DocumentNumber documentNumber,
        string fullName,
        DateOnly birthDate,
        LicenseCategory licenseCategory,
        DateOnly licenseIssuedOn,
        DateOnly licenseExpiresOn,
        DateTime utcNow)
    {
        var driver = new Driver(Guid.CreateVersion7())
        {
            DocumentNumber = documentNumber,
            BirthDate = birthDate,
            Status = DriverStatus.Active,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            FullName = NormalizeFullName(fullName),
            License = DriverLicense.Create(licenseCategory, licenseIssuedOn, licenseExpiresOn, birthDate),
        };

        driver.Raise(new DriverRegistered(driver.Id, documentNumber.Value));

        return driver;
    }

    public void Update(string fullName, DateTime utcNow)
    {
        EnsureNotDeleted();

        FullName = NormalizeFullName(fullName);
        UpdatedAt = utcNow;
    }

    // La licencia se valida contra la fecha de nacimiento que ya tiene el conductor.
    public void RenewLicense(LicenseCategory category, DateOnly issuedOn, DateOnly expiresOn, DateTime utcNow)
    {
        EnsureNotDeleted();

        License = DriverLicense.Create(category, issuedOn, expiresOn, BirthDate);
        UpdatedAt = utcNow;
    }

    public void ChangeStatus(DriverStatus newStatus, DateTime utcNow)
    {
        EnsureNotDeleted();

        if (newStatus == Status)
        {
            return;
        }

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = utcNow;

        Raise(new DriverStatusChanged(Id, previousStatus, newStatus));
    }

    public void Delete(DateTime utcNow)
    {
        // Idempotente: borrar un conductor ya borrado no cambia nada ni vuelve a registrar el evento.
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = utcNow;
        UpdatedAt = utcNow;

        Raise(new DriverDeleted(Id));
    }

    private void EnsureNotDeleted()
    {
        if (DeletedAt is not null)
        {
            throw new DomainException("Un conductor dado de baja no se puede modificar.");
        }
    }

    private static string NormalizeFullName(string? fullName)
    {
        var normalized = fullName?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            throw new DomainException("El nombre del conductor es obligatorio.");
        }

        return normalized;
    }
}
