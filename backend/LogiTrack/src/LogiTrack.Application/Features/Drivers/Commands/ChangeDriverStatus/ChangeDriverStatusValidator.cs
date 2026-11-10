using FluentValidation;

namespace LogiTrack.Application.Features.Drivers.Commands.ChangeDriverStatus;

public sealed class ChangeDriverStatusValidator : AbstractValidator<ChangeDriverStatusCommand>
{
    public ChangeDriverStatusValidator()
    {
        RuleFor(command => command.Status)
            .IsInEnum()
            .WithMessage("El estado del conductor no es válido.");
    }
}
