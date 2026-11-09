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

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    // Opcional. Guarda la referencia a la imagen; el archivo lo gestionará la feature Documents (fuera de esta fase).
    public string? ProfilePhotoUrl { get; private set; }

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
        string firstName,
        string lastName,
        string address,
        string phone,
        string email,
        DateOnly birthDate,
        LicenseCategory licenseCategory,
        DateOnly licenseIssuedOn,
        DateOnly licenseExpiresOn,
        DateTime utcNow,
        string? profilePhotoUrl = null)
    {
        var driver = new Driver(Guid.CreateVersion7())
        {
            DocumentNumber = documentNumber,
            BirthDate = birthDate,
            Status = DriverStatus.Active,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            License = DriverLicense.Create(licenseCategory, licenseIssuedOn, licenseExpiresOn, birthDate),
        };

        driver.ApplyContact(firstName, lastName, address, phone, email, profilePhotoUrl);

        driver.Raise(new DriverRegistered(driver.Id, documentNumber.Value));

        return driver;
    }

    // Reemplaza los datos de contacto. Una foto ausente (null) quita la foto anterior.
    public void Update(
        string firstName,
        string lastName,
        string address,
        string phone,
        string email,
        DateTime utcNow,
        string? profilePhotoUrl = null)
    {
        EnsureNotDeleted();

        ApplyContact(firstName, lastName, address, phone, email, profilePhotoUrl);
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

    private void ApplyContact(
        string firstName,
        string lastName,
        string address,
        string phone,
        string email,
        string? profilePhotoUrl)
    {
        FirstName = Required(firstName, "El nombre del conductor es obligatorio.");
        LastName = Required(lastName, "Los apellidos del conductor son obligatorios.");
        Address = Required(address, "La dirección del conductor es obligatoria.");
        Phone = Required(phone, "El teléfono del conductor es obligatorio.");
        Email = Required(email, "El correo del conductor es obligatorio.");
        ProfilePhotoUrl = string.IsNullOrWhiteSpace(profilePhotoUrl) ? null : profilePhotoUrl.Trim();
    }

    private void EnsureNotDeleted()
    {
        if (DeletedAt is not null)
        {
            throw new DomainException("Un conductor dado de baja no se puede modificar.");
        }
    }

    private static string Required(string? value, string message)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            throw new DomainException(message);
        }

        return normalized;
    }
}
