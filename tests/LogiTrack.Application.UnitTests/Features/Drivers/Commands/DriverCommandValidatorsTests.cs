using LogiTrack.Application.Features.Drivers.Commands.ChangeDriverStatus;
using LogiTrack.Application.Features.Drivers.Commands.CreateDriver;
using LogiTrack.Application.Features.Drivers.Commands.UpdateDriver;
using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.UnitTests.Features.Drivers.Commands;

public sealed class DriverCommandValidatorsTests
{
    private static readonly DateOnly BirthDate = new(1981, 1, 1);
    private static readonly DateOnly LicenseIssuedOn = new(2026, 1, 15);
    private static readonly DateOnly LicenseExpiresOn = new(2029, 1, 15);

    [Fact]
    public void CreateDriverValidator_WithValidCommand_IsValid()
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateDriverValidator_WithoutProfilePhoto_IsValid()
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand() with { ProfilePhotoUrl = null });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateDriverValidator_WithEmptyDocumentNumber_ReportsRequired()
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand() with { DocumentNumber = string.Empty });

        Assert.Contains(result.Errors, error => error.PropertyName == "DocumentNumber" && error.ErrorMessage == "La cédula es obligatoria.");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901")]
    [InlineData("12.345.678")]
    [InlineData("ABC12345")]
    public void CreateDriverValidator_WithInvalidDocumentNumber_ReportsFormat(string documentNumber)
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand() with { DocumentNumber = documentNumber });

        Assert.Contains(result.Errors, error => error.PropertyName == "DocumentNumber"
            && error.ErrorMessage == "La cédula debe tener entre 6 y 10 dígitos, sin puntos ni letras.");
    }

    [Fact]
    public void CreateDriverValidator_WithInvalidEmail_ReportsEmailFormat()
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand() with { Email = "no-es-correo" });

        Assert.Contains(result.Errors, error => error.PropertyName == "Email" && error.ErrorMessage == "El correo no tiene un formato válido.");
    }

    [Fact]
    public void CreateDriverValidator_WithBlankContactFields_ReportsEachField()
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand() with
        {
            FirstName = " ",
            LastName = string.Empty,
            Address = string.Empty,
            Phone = " ",
        });

        Assert.Contains(result.Errors, error => error.PropertyName == "FirstName");
        Assert.Contains(result.Errors, error => error.PropertyName == "LastName");
        Assert.Contains(result.Errors, error => error.PropertyName == "Address");
        Assert.Contains(result.Errors, error => error.PropertyName == "Phone");
    }

    [Fact]
    public void CreateDriverValidator_WithUndefinedLicenseCategory_Fails()
    {
        var result = new CreateDriverValidator().Validate(ValidCreateCommand() with { LicenseCategory = (LicenseCategory)99 });

        Assert.Contains(result.Errors, error => error.PropertyName == "LicenseCategory");
    }

    [Fact]
    public void UpdateDriverValidator_WithValidCommand_IsValid()
    {
        var result = new UpdateDriverValidator().Validate(ValidUpdateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateDriverValidator_WithEmptyFirstName_Fails()
    {
        var result = new UpdateDriverValidator().Validate(ValidUpdateCommand() with { FirstName = string.Empty });

        Assert.Contains(result.Errors, error => error.PropertyName == "FirstName");
    }

    [Fact]
    public void UpdateDriverValidator_WithPhotoReferenceTooLong_Fails()
    {
        var result = new UpdateDriverValidator().Validate(ValidUpdateCommand() with { ProfilePhotoUrl = new string('a', 501) });

        Assert.Contains(result.Errors, error => error.PropertyName == "ProfilePhotoUrl");
    }

    [Fact]
    public void ChangeDriverStatusValidator_WithUndefinedStatus_Fails()
    {
        var command = new ChangeDriverStatusCommand(Guid.NewGuid(), 1, (DriverStatus)99);

        var result = new ChangeDriverStatusValidator().Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Status");
    }

    private static CreateDriverCommand ValidCreateCommand() => new(
        "1234567890",
        "Ana",
        "Gómez Ruiz",
        "Calle 10 # 20-30",
        "3001234567",
        "ana@example.com",
        BirthDate,
        LicenseCategory.C2,
        LicenseIssuedOn,
        LicenseExpiresOn,
        null);

    private static UpdateDriverCommand ValidUpdateCommand() => new(
        Guid.NewGuid(),
        1,
        "Ana",
        "Gómez Ruiz",
        "Calle 10 # 20-30",
        "3001234567",
        "ana@example.com",
        null,
        LicenseCategory.C2,
        LicenseIssuedOn,
        LicenseExpiresOn);
}
