using Mediator;

namespace LogiTrack.Application.Features.Users.Commands.CreateUser;

// Role es el código de un rol de security.roles (por ejemplo "Operator"). Se valida contra la base de datos.
public sealed record CreateUserCommand(string Email, string Password, string Role) : ICommand<UserDetailsResponse>;
