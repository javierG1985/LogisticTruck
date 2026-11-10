using FluentValidation;

namespace LogiTrack.Application.Features.Trucks.Commands.UpdateTruck;

public sealed class UpdateTruckValidator : AbstractValidator<UpdateTruckCommand>
{
    public UpdateTruckValidator()
    {
        RuleFor(command => command.Brand)
            .NotEmpty()
            .WithMessage("La marca es obligatoria.")
            .MaximumLength(60)
            .WithMessage("La marca no puede superar 60 caracteres.");

        RuleFor(command => command.TruckType)
            .IsInEnum()
            .WithMessage("El tipo de camión debe ser Light, Rigid o Articulated.");

        RuleFor(command => command.ModelYear)
            .InclusiveBetween(1950, 2100)
            .WithMessage("El año modelo debe estar entre 1950 y 2100.");
    }
}
