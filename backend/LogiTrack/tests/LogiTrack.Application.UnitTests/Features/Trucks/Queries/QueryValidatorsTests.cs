using LogiTrack.Application.Features.Trucks.Queries.ListTrucks;

namespace LogiTrack.Application.UnitTests.Features.Trucks.Queries;

public sealed class QueryValidatorsTests
{
    [Fact]
    public void ListTrucksValidator_WithPageZero_Fails()
    {
        var result = new ListTrucksValidator().Validate(new ListTrucksQuery { Page = 0 });

        Assert.Contains(result.Errors, error => error.PropertyName == "Page");
    }

    [Fact]
    public void ListTrucksValidator_WithPageSizeAbove100_Fails()
    {
        var result = new ListTrucksValidator().Validate(new ListTrucksQuery { PageSize = 101 });

        Assert.Contains(result.Errors, error => error.PropertyName == "PageSize");
    }

    [Fact]
    public void ListTrucksValidator_WithUnknownSortField_Fails()
    {
        var result = new ListTrucksValidator().Validate(new ListTrucksQuery { Sort = "color" });

        Assert.Contains(result.Errors, error => error.PropertyName == "Sort");
    }

    [Fact]
    public void ListTrucksValidator_WithDescendingSort_IsValid()
    {
        var result = new ListTrucksValidator().Validate(new ListTrucksQuery { Sort = "plate,-createdAt" });

        Assert.True(result.IsValid);
    }
}
