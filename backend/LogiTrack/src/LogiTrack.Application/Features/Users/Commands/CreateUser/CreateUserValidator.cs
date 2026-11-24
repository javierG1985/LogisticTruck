using FluentValidation;

namespace LogiTrack.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("El correo es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(254)
            .WithMessage("El correo no puede superar 254 caracteres.");

        // Mínimo de 8 caracteres, sin reglas de composición: la longitud pesa más que los símbolos (NIST SP 800-63B).
        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(256)
            .WithMessage("La contraseña no puede superar 256 caracteres.");

        RuleFor(command => command.Role)
            .NotEmpty()
            .WithMessage("El rol es obligatorio.")
            .MaximumLength(60)
            .WithMessage("El rol no puede superar 60 caracteres.");
    }
}
