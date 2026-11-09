using FluentValidation;

namespace LogiTrack.Application.Features.Drivers.Commands.UpdateDriver;

public sealed class UpdateDriverValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverValidator()
    {
        RuleFor(command => command.FirstName)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .MaximumLength(60)
            .WithMessage("El nombre no puede superar 60 caracteres.");

        RuleFor(command => command.LastName)
            .NotEmpty()
            .WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(60)
            .WithMessage("Los apellidos no pueden superar 60 caracteres.");

        RuleFor(command => command.Address)
            .NotEmpty()
            .WithMessage("La dirección es obligatoria.")
            .MaximumLength(200)
            .WithMessage("La dirección no puede superar 200 caracteres.");

        RuleFor(command => command.Phone)
            .NotEmpty()
            .WithMessage("El teléfono es obligatorio.")
            .MaximumLength(20)
            .WithMessage("El teléfono no puede superar 20 caracteres.");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("El correo es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(254)
            .WithMessage("El correo no puede superar 254 caracteres.");

        RuleFor(command => command.ProfilePhotoUrl)
            .MaximumLength(500)
            .WithMessage("La referencia a la foto no puede superar 500 caracteres.");

        RuleFor(command => command.LicenseCategory)
            .IsInEnum()
            .WithMessage("La categoría de licencia no es válida.");
    }
}
