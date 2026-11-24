using FluentValidation;
using FluentValidation.Results;
using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Application.Common.Security;
using LogiTrack.Domain.Aggregates.Users;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, TimeProvider timeProvider)
    : ICommandHandler<CreateUserCommand, UserDetailsResponse>
{
    public async ValueTask<UserDetailsResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(command.Email);

        // Los roles viven en la base de datos: si el código no existe, es un error de la petición.
        var roleExists = await db.Roles.AnyAsync(role => role.Code == command.Role, cancellationToken);

        if (!roleExists)
        {
            throw new ValidationException(
                "Hay errores de validación en la solicitud.",
                [new ValidationFailure(nameof(CreateUserCommand.Role), "El rol no existe.")]);
        }

        var exists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);

        if (exists)
        {
            throw new ConflictException("Ya existe un usuario con ese correo.");
        }

        var user = User.Create(
            command.Email,
            passwordHasher.Hash(command.Password),
            command.Role,
            timeProvider.GetUtcNow().UtcDateTime);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return UserDetailsResponse.From(user);
    }
}
