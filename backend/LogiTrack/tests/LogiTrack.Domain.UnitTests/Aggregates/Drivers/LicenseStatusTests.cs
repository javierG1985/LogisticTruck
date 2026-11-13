using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Domain.UnitTests.Aggregates.Drivers;

public sealed class LicenseStatusTests
{
    private static readonly DateOnly Today = new(2026, 11, 13);
    private static readonly DateOnly ExpiresOn = new(2027, 11, 13);

    [Fact]
    public void StatusFor_MoreThanSixtyDaysLeft_IsValid()
    {
        Assert.Equal(LicenseStatus.Valid, DriverLicense.StatusFor(Today.AddDays(61), Today));
    }

    [Fact]
    public void StatusFor_SixtyDaysLeft_IsExpiringSoon()
    {
        Assert.Equal(LicenseStatus.ExpiringSoon, DriverLicense.StatusFor(Today.AddDays(60), Today));
    }

    [Fact]
    public void StatusFor_ExpiryDay_IsStillExpiringSoonNotExpired()
    {
        Assert.Equal(LicenseStatus.ExpiringSoon, DriverLicense.StatusFor(Today, Today));
    }

    [Fact]
    public void StatusFor_DayAfterExpiry_IsExpired()
    {
        Assert.Equal(LicenseStatus.Expired, DriverLicense.StatusFor(Today.AddDays(-1), Today));
    }

    [Theory]
    [InlineData(LicenseCategory.C1)]
    [InlineData(LicenseCategory.C2)]
    [InlineData(LicenseCategory.C3)]
    public void StatusOn_IsTheSameForEveryCategory(LicenseCategory category)
    {
        var license = DriverLicense.Create(category, Today.AddYears(-2), Today.AddDays(30), new DateOnly(1981, 1, 1));

        Assert.Equal(LicenseStatus.ExpiringSoon, license.StatusOn(Today));
    }
}
