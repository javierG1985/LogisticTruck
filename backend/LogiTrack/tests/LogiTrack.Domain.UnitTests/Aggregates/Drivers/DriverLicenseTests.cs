using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Aggregates.Drivers;

public sealed class DriverLicenseTests
{
    [Fact]
    public void Create_ThreeYearsForDriverUnder60_IsAccepted()
    {
        // Conductor de 45 años al expedir: 3 años es el máximo.
        var license = DriverLicense.Create(LicenseCategory.C2, new(2026, 3, 10), new(2029, 3, 10), new(1981, 3, 10));

        Assert.Equal(new DateOnly(2029, 3, 10), license.ExpiresOn);
    }

    [Fact]
    public void Create_ThreeYearsAndOneDayForDriverUnder60_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => DriverLicense.Create(
            LicenseCategory.C2, new(2026, 3, 10), new(2029, 3, 11), new(1981, 3, 10)));
    }

    [Fact]
    public void Create_OneYearForDriverOf62_IsAccepted()
    {
        var license = DriverLicense.Create(LicenseCategory.C2, new(2026, 6, 1), new(2027, 6, 1), new(1964, 1, 1));

        Assert.Equal(new DateOnly(2027, 6, 1), license.ExpiresOn);
    }

    [Fact]
    public void Create_ThirteenMonthsForDriverOf62_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => DriverLicense.Create(
            LicenseCategory.C2, new(2026, 6, 1), new(2027, 7, 1), new(1964, 1, 1)));
    }

    [Fact]
    public void Create_OnTheDayDriverTurns60_UsesOneYearLimit()
    {
        // Cumple 60 justo el día de la expedición: ya cuenta como 60 o más, así que 1 año.
        var birthDate = new DateOnly(1966, 11, 9);
        var issuedOn = new DateOnly(2026, 11, 9);

        var license = DriverLicense.Create(LicenseCategory.C1, issuedOn, new(2027, 11, 9), birthDate);

        Assert.Equal(new DateOnly(2027, 11, 9), license.ExpiresOn);
        Assert.Throws<DomainException>(() => DriverLicense.Create(
            LicenseCategory.C1, issuedOn, new(2027, 11, 10), birthDate));
    }

    [Fact]
    public void Create_TheDayBeforeDriverTurns60_UsesThreeYearLimit()
    {
        var birthDate = new DateOnly(1966, 11, 9);
        var issuedOn = new DateOnly(2026, 11, 8);

        var license = DriverLicense.Create(LicenseCategory.C1, issuedOn, new(2029, 11, 8), birthDate);

        Assert.Equal(new DateOnly(2029, 11, 8), license.ExpiresOn);
    }

    [Fact]
    public void Create_ExpiringOnTheIssueDate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => DriverLicense.Create(
            LicenseCategory.C1, new(2026, 3, 10), new(2026, 3, 10), new(1981, 3, 10)));
    }

    [Fact]
    public void Create_IssuedBeforeBirthDate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => DriverLicense.Create(
            LicenseCategory.C1, new(2000, 1, 1), new(2003, 1, 1), new(2001, 1, 1)));
    }

    [Fact]
    public void IsValidOn_BetweenIssueAndExpiryDatesInclusive_IsTrue()
    {
        var license = DriverLicense.Create(LicenseCategory.C2, new(2026, 3, 10), new(2027, 3, 10), new(1981, 3, 10));

        Assert.True(license.IsValidOn(new DateOnly(2026, 3, 10)));
        Assert.True(license.IsValidOn(new DateOnly(2027, 3, 10)));
    }

    [Fact]
    public void IsValidOn_TheDayAfterExpiry_IsFalse()
    {
        var license = DriverLicense.Create(LicenseCategory.C2, new(2026, 3, 10), new(2027, 3, 10), new(1981, 3, 10));

        Assert.False(license.IsValidOn(new DateOnly(2027, 3, 11)));
    }

    [Theory]
    [InlineData(LicenseCategory.C3, LicenseCategory.C3, true)]
    [InlineData(LicenseCategory.C3, LicenseCategory.C2, true)]
    [InlineData(LicenseCategory.C3, LicenseCategory.C1, true)]
    [InlineData(LicenseCategory.C2, LicenseCategory.C1, true)]
    [InlineData(LicenseCategory.C1, LicenseCategory.C1, true)]
    [InlineData(LicenseCategory.C2, LicenseCategory.C3, false)]
    [InlineData(LicenseCategory.C1, LicenseCategory.C2, false)]
    public void Allows_FollowsCategoryHierarchy(LicenseCategory held, LicenseCategory required, bool expected)
    {
        var license = DriverLicense.Create(held, new(2026, 3, 10), new(2027, 3, 10), new(1981, 3, 10));

        Assert.Equal(expected, license.Allows(required));
    }
}
