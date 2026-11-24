using FluentValidation;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Application.Features.Users.Commands.CreateUser;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogiTrack.Api.Bootstrap;

// Crea el primer administrador con `dotnet run -- --bootstrap-admin`. Lee Bootstrap__AdminEmail y Bootstrap__AdminPassword del entorno.
// Es idempotente: si el correo ya existe, no cambia nada. El rol "Administrator" debe existir (lo siembra la migración).
public static class AdminBootstrapper
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var email = configuration["Bootstrap:AdminEmail"]
            ?? throw new InvalidOperationException("Define Bootstrap__AdminEmail para crear el administrador.");

        var password = configuration["Bootstrap:AdminPassword"]
            ?? throw new InvalidOperationException("Define Bootstrap__AdminPassword para crear el administrador.");

        var command = new CreateUserCommand(email, password, "Administrator");
        new CreateUserValidator().ValidateAndThrow(command);

        await using var scope = services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            await sender.Send(command, cancellationToken);
            Console.WriteLine($"Administrador creado: {email}");
        }
        catch (ConflictException)
        {
            Console.WriteLine("El correo ya tiene un usuario; no se hizo ningún cambio.");
        }
    }
}
