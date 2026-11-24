using FluentValidation;

namespace LogiTrack.Application.Features.Drivers.Commands.RenewDriverLicense;

// Solo la forma: las fechas y su coherencia con la norma las revisa Driver y DriverLicense.
public sealed class RenewDriverLicenseValidator : AbstractValidator<RenewDriverLicenseCommand>
{
    public RenewDriverLicenseValidator()
    {
        RuleFor(command => command.LicenseCategory)
            .IsInEnum()
            .WithMessage("La categoría de licencia no es válida.");
    }
}
