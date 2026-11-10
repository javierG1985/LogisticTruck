using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Aggregates.Assignments;

public sealed class TruckAssignmentPolicyTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = new(2026, 2, 1);
    private static readonly DateOnly BirthDate = new(1981, 1, 1);
    private static readonly DateOnly SoatExpiresOn = new(2027, 1, 31);
    private static readonly DateOnly TechReviewExpiresOn = new(2027, 6, 30);

    [Fact]
    public void EnsureCanAssign_AvailableTruckAndValidDriver_Passes()
    {
        var truck = NewTruck(TruckType.Rigid);
        var driver = NewDriver(LicenseCategory.C2);

        TruckAssignmentPolicy.EnsureCanAssign(truck, driver, Today);
    }

    [Theory]
    [InlineData(TruckType.Light, LicenseCategory.C1)]
    [InlineData(TruckType.Rigid, LicenseCategory.C2)]
    [InlineData(TruckType.Articulated, LicenseCategory.C3)]
    public void RequiredLicenseFor_MapsEachTruckTypeToItsMinimumCategory(TruckType truckType, LicenseCategory expected)
    {
        Assert.Equal(expected, TruckAssignmentPolicy.RequiredLicenseFor(truckType));
    }

    [Fact]
    public void EnsureCanAssign_HigherCategoryCoversLowerTruck()
    {
        var truck = NewTruck(TruckType.Light);
        var driver = NewDriver(LicenseCategory.C3);

        TruckAssignmentPolicy.EnsureCanAssign(truck, driver, Today);
    }

    [Fact]
    public void EnsureCanAssign_DriverWithLowerCategoryThanTruckNeeds_IsRejected()
    {
        var truck = NewTruck(TruckType.Articulated);
        var driver = NewDriver(LicenseCategory.C2);

        var exception = Assert.Throws<DomainException>(() => TruckAssignmentPolicy.EnsureCanAssign(truck, driver, Today));

        Assert.Equal("La licencia del conductor no alcanza la categoría que exige el camión.", exception.Message);
    }

    [Theory]
    [InlineData(TruckStatus.InRoute)]
    [InlineData(TruckStatus.Maintenance)]
    [InlineData(TruckStatus.Inactive)]
    public void EnsureCanAssign_TruckNotAvailable_IsRejected(TruckStatus status)
    {
        var truck = NewTruck(TruckType.Rigid);
        truck.ChangeStatus(status, Now);

        Assert.Throws<DomainException>(() => TruckAssignmentPolicy.EnsureCanAssign(truck, NewDriver(LicenseCategory.C2), Today));
    }

    [Fact]
    public void EnsureCanAssign_DeletedTruck_IsRejected()
    {
        var truck = NewTruck(TruckType.Rigid);
        truck.Delete(Now);

        Assert.Throws<DomainException>(() => TruckAssignmentPolicy.EnsureCanAssign(truck, NewDriver(LicenseCategory.C2), Today));
    }

    [Theory]
    [InlineData(DriverStatus.OnLeave)]
    [InlineData(DriverStatus.Inactive)]
    public void EnsureCanAssign_DriverNotActive_IsRejected(DriverStatus status)
    {
        var driver = NewDriver(LicenseCategory.C2);
        driver.ChangeStatus(status, Now);

        Assert.Throws<DomainException>(() => TruckAssignmentPolicy.EnsureCanAssign(NewTruck(TruckType.Rigid), driver, Today));
    }

    [Fact]
    public void EnsureCanAssign_DeletedDriver_IsRejected()
    {
        var driver = NewDriver(LicenseCategory.C2);
        driver.Delete(Now);

        Assert.Throws<DomainException>(() => TruckAssignmentPolicy.EnsureCanAssign(NewTruck(TruckType.Rigid), driver, Today));
    }

    [Fact]
    public void EnsureCanAssign_ExpiredLicense_IsRejected()
    {
        var driver = NewDriver(LicenseCategory.C2);

        // La licencia vence el 15/01/2029; el día después ya no está vigente.
        var afterExpiry = new DateOnly(2029, 1, 16);

        var exception = Assert.Throws<DomainException>(() =>
            TruckAssignmentPolicy.EnsureCanAssign(NewTruck(TruckType.Rigid), driver, afterExpiry));

        Assert.Equal("La licencia del conductor no está vigente.", exception.Message);
    }

    private static Truck NewTruck(TruckType truckType) =>
        Truck.Register(
            Plate.Create("ABC123"), "Kenworth", truckType, 2022, 8000m, 30m, SoatExpiresOn, TechReviewExpiresOn, Now);

    private static Driver NewDriver(LicenseCategory category) =>
        Driver.Register(
            DocumentNumber.Create("1234567890"),
            "Ana", "Gómez Ruiz", "Calle 10 # 20-30", "3001234567", "ana@example.com",
            BirthDate, category, new DateOnly(2026, 1, 15), new DateOnly(2029, 1, 15), Now);
}
