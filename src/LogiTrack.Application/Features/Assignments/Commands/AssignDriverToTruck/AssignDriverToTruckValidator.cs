using FluentValidation;

namespace LogiTrack.Application.Features.Assignments.Commands.AssignDriverToTruck;

// Solo revisa la forma. Que el camión y el conductor puedan asignarse lo decide la política de dominio.
public sealed class AssignDriverToTruckValidator : AbstractValidator<AssignDriverToTruckCommand>
{
    public AssignDriverToTruckValidator()
    {
        RuleFor(command => command.DriverId)
            .NotEmpty()
            .WithMessage("El conductor es obligatorio.");
    }
}
