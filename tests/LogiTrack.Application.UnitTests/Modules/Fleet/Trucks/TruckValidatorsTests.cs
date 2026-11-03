using LogiTrack.Application.Modules.Fleet.Trucks.ChangeTruckStatus;
using LogiTrack.Application.Modules.Fleet.Trucks.CreateTruck;
using LogiTrack.Application.Modules.Fleet.Trucks.ListTrucks;
using LogiTrack.Application.Modules.Fleet.Trucks.UpdateTruck;
using LogiTrack.Domain.Modules.Fleet.Trucks;

namespace LogiTrack.Application.UnitTests.Modules.Fleet.Trucks;

public sealed class TruckValidatorsTests
{
    private static readonly DateOnly SoatExpiresOn = new(2027, 1, 31);
    private static readonly DateOnly TechReviewExpiresOn = new(2027, 6, 30);

    [Fact]
    public void CreateTruckValidator_WithValidCommand_IsValid()
    {
        var result = new CreateTruckValidator().Validate(ValidCreateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTruckValidator_WithEmptyPlate_ReportsPlateRequired()
    {
        var result = new CreateTruckValidator().Validate(ValidCreateCommand() with { Plate = string.Empty });

        Assert.Contains(result.Errors, error => error.PropertyName == "Plate" && error.ErrorMessage == "La placa es obligatoria.");
    }

    [Fact]
    public void CreateTruckValidator_WithInvalidPlateFormat_ReportsPlateFormat()
    {
        var result = new CreateTruckValidator().Validate(ValidCreateCommand() with { Plate = "AB12" });

        Assert.Contains(result.Errors, error => error.PropertyName == "Plate" && error.ErrorMessage == "La placa debe tener el formato ABC123.");
    }

    [Fact]
    public void CreateTruckValidator_WithEmptyBrand_ReportsBrandRequired()
    {
        var result = new CreateTruckValidator().Validate(ValidCreateCommand() with { Brand = " " });

        Assert.Contains(result.Errors, error => error.PropertyName == "Brand");
    }

    [Fact]
    public void CreateTruckValidator_WithModelYearOutOfRange_Fails()
    {
        var result = new CreateTruckValidator().Validate(ValidCreateCommand() with { ModelYear = 1800 });

        Assert.Contains(result.Errors, error => error.PropertyName == "ModelYear");
    }

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

    [Fact]
    public void UpdateTruckValidator_WithEmptyBrand_Fails()
    {
        var command = new UpdateTruckCommand(Guid.NewGuid(), 1, string.Empty, 2022, 8000m, 30m, SoatExpiresOn, TechReviewExpiresOn);

        var result = new UpdateTruckValidator().Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Brand");
    }

    [Fact]
    public void ChangeTruckStatusValidator_WithUndefinedStatus_Fails()
    {
        var command = new ChangeTruckStatusCommand(Guid.NewGuid(), 1, (TruckStatus)99);

        var result = new ChangeTruckStatusValidator().Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Status");
    }

    private static CreateTruckCommand ValidCreateCommand() =>
        new("ABC123", "Kenworth", 2022, 8000m, 30m, SoatExpiresOn, TechReviewExpiresOn);
}
