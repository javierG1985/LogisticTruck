using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Application.Common.Security;
using LogiTrack.Domain.Aggregates.Users;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Auth.Login;

public sealed class LoginHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, ITokenIssuer tokenIssuer)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async ValueTask<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(command.Email);

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthenticatedException("Correo o contraseña no válidos.");
        }

        var permissions = await db.RolePrivileges
            .AsNoTracking()
            .Where(rolePermission => rolePermission.RoleCode == user.RoleCode)
            .Select(rolePermission => rolePermission.PermissionCode)
            .ToListAsync(cancellationToken);

        var token = tokenIssuer.Issue(user.Id, user.Email, user.RoleCode, permissions);

        return new LoginResponse(token.AccessToken, token.ExpiresInSeconds, user.Email, user.RoleCode);
    }
}
