using FluentValidation;
using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.Features.Drivers.Commands.CreateDriver;

// Solo revisa la forma de la entrada. El formato de la cédula vive en DocumentNumber: se consulta, no se repite.
public sealed class CreateDriverValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverValidator()
    {
        RuleFor(command => command.DocumentNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("La cédula es obligatoria.")
            .Must(documentNumber => DocumentNumber.TryCreate(documentNumber, out _))
            .WithMessage("La cédula debe tener entre 6 y 10 dígitos, sin puntos ni letras.");

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
