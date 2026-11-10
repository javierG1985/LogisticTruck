using FluentValidation;
using LogiTrack.Application.Features.Drivers;
using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.Features.Drivers.Commands.CreateDriver;

public sealed class CreateDriverValidator : DriverProfileValidator<CreateDriverCommand>
{
    public CreateDriverValidator()
    {
        // El formato de la cédula vive en DocumentNumber: se consulta, no se repite.
        RuleFor(command => command.DocumentNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("La cédula es obligatoria.")
            .Must(documentNumber => DocumentNumber.TryCreate(documentNumber, out _))
            .WithMessage("La cédula debe tener entre 6 y 10 dígitos, sin puntos ni letras.");
    }
}
