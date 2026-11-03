using FluentValidation;
using LogiTrack.Domain.Modules.Fleet.Trucks;

namespace LogiTrack.Application.Modules.Fleet.Trucks.CreateTruck;

// Solo revisa la forma de la entrada. El formato de la placa vive en Plate: se consulta, no se repite.
public sealed class CreateTruckValidator : AbstractValidator<CreateTruckCommand>
{
    public CreateTruckValidator()
    {
        RuleFor(command => command.Plate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("La placa es obligatoria.")
            .Must(plate => Plate.TryCreate(plate, out _))
            .WithMessage("La placa debe tener el formato ABC123.");

        RuleFor(command => command.Brand)
            .NotEmpty()
            .WithMessage("La marca es obligatoria.")
            .MaximumLength(60)
            .WithMessage("La marca no puede superar 60 caracteres.");

        RuleFor(command => command.ModelYear)
            .InclusiveBetween(1950, 2100)
            .WithMessage("El año modelo debe estar entre 1950 y 2100.");
    }
}
