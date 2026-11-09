using LogiTrack.Application.Features.Drivers.Queries.ListDrivers;

namespace LogiTrack.Application.UnitTests.Features.Drivers.Queries;

public sealed class DriverQueryValidatorsTests
{
    [Fact]
    public void ListDriversValidator_WithValidSort_IsValid()
    {
        var result = new ListDriversValidator().Validate(new ListDriversQuery { Sort = "lastName,-createdAt" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ListDriversValidator_WithUnknownSortField_Fails()
    {
        var result = new ListDriversValidator().Validate(new ListDriversQuery { Sort = "salary" });

        Assert.Contains(result.Errors, error => error.PropertyName == "Sort");
    }

    [Fact]
    public void ListDriversValidator_WithPageSizeAboveLimit_Fails()
    {
        var result = new ListDriversValidator().Validate(new ListDriversQuery { PageSize = 101 });

        Assert.Contains(result.Errors, error => error.PropertyName == "PageSize");
    }
}
