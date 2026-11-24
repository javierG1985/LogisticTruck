using FluentValidation;

namespace LogiTrack.Application.Features.Auth.Login;

// Solo la forma. Si el correo o la contraseña son incorrectos, lo decide el handler con un mensaje genérico.
public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("El correo es obligatorio.")
            .MaximumLength(254)
            .WithMessage("El correo no puede superar 254 caracteres.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MaximumLength(256)
            .WithMessage("La contraseña no puede superar 256 caracteres.");
    }
}
