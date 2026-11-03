using FluentValidation;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ChangeTruckStatus;

public sealed class ChangeTruckStatusValidator : AbstractValidator<ChangeTruckStatusCommand>
{
    public ChangeTruckStatusValidator()
    {
        RuleFor(command => command.Status)
            .IsInEnum()
            .WithMessage("El estado del camión no es válido.");
    }
}
