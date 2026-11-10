using FluentValidation;

namespace LogiTrack.Application.Features.Trucks.Commands.ChangeTruckStatus;

public sealed class ChangeTruckStatusValidator : AbstractValidator<ChangeTruckStatusCommand>
{
    public ChangeTruckStatusValidator()
    {
        RuleFor(command => command.Status)
            .IsInEnum()
            .WithMessage("El estado del camión no es válido.");
    }
}
