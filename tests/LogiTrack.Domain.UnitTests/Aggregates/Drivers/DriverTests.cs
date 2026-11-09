using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Drivers.Events;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Aggregates.Drivers;

public sealed class DriverTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly BirthDate = new(1981, 1, 1); // 44 años al expedir la licencia.
    private static readonly DateOnly LicenseIssuedOn = new(2026, 1, 15);
    private static readonly DateOnly LicenseExpiresOn = new(2029, 1, 15);

    [Fact]
    public void Register_NewDriver_IsActiveAndRaisesDriverRegistered()
    {
        var driver = CreateDriver();

        Assert.Equal(DriverStatus.Active, driver.Status);
        Assert.Equal("Ana", driver.FirstName);
        Assert.Equal("Gómez Ruiz", driver.LastName);
        Assert.Equal("Calle 10 # 20-30", driver.Address);
        Assert.Equal("3001234567", driver.Phone);
        Assert.Equal("ana@example.com", driver.Email);

        var domainEvent = Assert.IsType<DriverRegistered>(Assert.Single(driver.DomainEvents));
        Assert.Equal(driver.Id, domainEvent.DriverId);
        Assert.Equal("1234567890", domainEvent.DocumentNumber);
    }

    [Fact]
    public void Register_WithoutProfilePhoto_LeavesItEmpty()
    {
        var driver = CreateDriver();

        Assert.Null(driver.ProfilePhotoUrl);
    }

    [Fact]
    public void Register_WithProfilePhoto_StoresTheReference()
    {
        var driver = Driver.Register(
            DocumentNumber.Create("1234567890"),
            "Ana", "Gómez Ruiz", "Calle 10 # 20-30", "3001234567", "ana@example.com",
            BirthDate, LicenseCategory.C2, LicenseIssuedOn, LicenseExpiresOn, Now,
            profilePhotoUrl: "  fotos/ana.jpg ");

        Assert.Equal("fotos/ana.jpg", driver.ProfilePhotoUrl);
    }

    [Fact]
    public void Register_WithLicenseBeyondLegalLimit_ThrowsDomainException()
    {
        // Conductor de 62 años: la licencia no puede durar más de 1 año.
        Assert.Throws<DomainException>(() => Driver.Register(
            DocumentNumber.Create("1234567890"),
            "Ana", "Gómez Ruiz", "Calle 10 # 20-30", "3001234567", "ana@example.com",
            new DateOnly(1964, 1, 1),
            LicenseCategory.C2,
            LicenseIssuedOn,
            LicenseExpiresOn,
            Now));
    }

    [Theory]
    [InlineData("", "Gómez Ruiz", "Calle 10", "3001234567", "ana@example.com", "El nombre del conductor es obligatorio.")]
    [InlineData("Ana", "  ", "Calle 10", "3001234567", "ana@example.com", "Los apellidos del conductor son obligatorios.")]
    [InlineData("Ana", "Gómez Ruiz", "", "3001234567", "ana@example.com", "La dirección del conductor es obligatoria.")]
    [InlineData("Ana", "Gómez Ruiz", "Calle 10", " ", "ana@example.com", "El teléfono del conductor es obligatorio.")]
    [InlineData("Ana", "Gómez Ruiz", "Calle 10", "3001234567", null, "El correo del conductor es obligatorio.")]
    public void Register_WithMissingContactField_ThrowsDomainException(
        string firstName, string lastName, string address, string phone, string? email, string expectedMessage)
    {
        var exception = Assert.Throws<DomainException>(() => Driver.Register(
            DocumentNumber.Create("1234567890"),
            firstName, lastName, address, phone, email ?? string.Empty,
            BirthDate, LicenseCategory.C2, LicenseIssuedOn, LicenseExpiresOn, Now));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void ChangeStatus_ToTheSameStatus_DoesNothing()
    {
        var driver = CreateDriver();
        driver.ClearDomainEvents();

        driver.ChangeStatus(DriverStatus.Active, Now);

        Assert.Empty(driver.DomainEvents);
    }

    [Fact]
    public void ChangeStatus_ToNewStatus_RaisesDriverStatusChangedWithPreviousAndNewStatus()
    {
        var driver = CreateDriver();
        driver.ClearDomainEvents();

        driver.ChangeStatus(DriverStatus.OnLeave, Now);

        Assert.Equal(DriverStatus.OnLeave, driver.Status);
        var domainEvent = Assert.IsType<DriverStatusChanged>(Assert.Single(driver.DomainEvents));
        Assert.Equal(DriverStatus.Active, domainEvent.PreviousStatus);
        Assert.Equal(DriverStatus.OnLeave, domainEvent.NewStatus);
    }

    [Fact]
    public void Update_ReplacesContactDataAndPhoto()
    {
        var driver = CreateDriver();

        driver.Update("Ana María", "Gómez", "Carrera 5 # 1-2", "3109876543", "nueva@example.com", Now, "fotos/ana.png");

        Assert.Equal("Ana María", driver.FirstName);
        Assert.Equal("Carrera 5 # 1-2", driver.Address);
        Assert.Equal("3109876543", driver.Phone);
        Assert.Equal("nueva@example.com", driver.Email);
        Assert.Equal("fotos/ana.png", driver.ProfilePhotoUrl);
    }

    [Fact]
    public void Update_WithoutPhoto_RemovesThePreviousPhoto()
    {
        var driver = Driver.Register(
            DocumentNumber.Create("1234567890"),
            "Ana", "Gómez Ruiz", "Calle 10 # 20-30", "3001234567", "ana@example.com",
            BirthDate, LicenseCategory.C2, LicenseIssuedOn, LicenseExpiresOn, Now,
            profilePhotoUrl: "fotos/ana.jpg");

        driver.Update("Ana", "Gómez Ruiz", "Calle 10 # 20-30", "3001234567", "ana@example.com", Now);

        Assert.Null(driver.ProfilePhotoUrl);
    }

    [Fact]
    public void RenewLicense_ValidatesAgainstTheDriverBirthDate()
    {
        // Conductor de 62 años: la renovación no puede durar más de 1 año, aunque la licencia anterior sí.
        var driver = Driver.Register(
            DocumentNumber.Create("1234567890"),
            "Ana", "Gómez Ruiz", "Calle 10 # 20-30", "3001234567", "ana@example.com",
            new DateOnly(1964, 1, 1),
            LicenseCategory.C2,
            new DateOnly(2026, 1, 15),
            new DateOnly(2027, 1, 15),
            Now);

        Assert.Throws<DomainException>(() => driver.RenewLicense(
            LicenseCategory.C2, new DateOnly(2027, 1, 15), new DateOnly(2028, 3, 15), Now));
    }

    [Fact]
    public void RenewLicense_WithValidLicense_ReplacesIt()
    {
        var driver = CreateDriver();

        driver.RenewLicense(LicenseCategory.C3, LicenseIssuedOn, new DateOnly(2028, 1, 15), Now);

        Assert.Equal(LicenseCategory.C3, driver.License.Category);
        Assert.Equal(new DateOnly(2028, 1, 15), driver.License.ExpiresOn);
    }

    [Fact]
    public void Delete_RaisesDriverDeletedOnlyOnce()
    {
        var driver = CreateDriver();
        driver.ClearDomainEvents();

        driver.Delete(Now);
        driver.Delete(Now.AddDays(1));

        Assert.NotNull(driver.DeletedAt);
        Assert.Equal(Now, driver.DeletedAt);
        var domainEvent = Assert.IsType<DriverDeleted>(Assert.Single(driver.DomainEvents));
        Assert.Equal(driver.Id, domainEvent.DriverId);
    }

    [Fact]
    public void Update_DeletedDriver_ThrowsDomainException()
    {
        var driver = CreateDriver();
        driver.Delete(Now);

        Assert.Throws<DomainException>(() => driver.Update(
            "Otro", "Nombre", "Calle 1", "3000000000", "otro@example.com", Now));
    }

    [Fact]
    public void ChangeStatus_DeletedDriver_ThrowsDomainException()
    {
        var driver = CreateDriver();
        driver.Delete(Now);

        Assert.Throws<DomainException>(() => driver.ChangeStatus(DriverStatus.Inactive, Now));
    }

    [Fact]
    public void RenewLicense_DeletedDriver_ThrowsDomainException()
    {
        var driver = CreateDriver();
        driver.Delete(Now);

        Assert.Throws<DomainException>(() => driver.RenewLicense(
            LicenseCategory.C1, LicenseIssuedOn, new DateOnly(2028, 1, 15), Now));
    }

    private static Driver CreateDriver() => Driver.Register(
        DocumentNumber.Create("1234567890"),
        "Ana",
        "Gómez Ruiz",
        "Calle 10 # 20-30",
        "3001234567",
        "ana@example.com",
        BirthDate,
        LicenseCategory.C2,
        LicenseIssuedOn,
        LicenseExpiresOn,
        Now);
}
