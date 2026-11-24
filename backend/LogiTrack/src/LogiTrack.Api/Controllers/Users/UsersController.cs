using LogiTrack.Application.Features.Users;
using LogiTrack.Application.Features.Users.Commands.CreateUser;
using LogiTrack.Application.Common.Security;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Users;

[ApiController]
[Route("api/users")]
[Tags("Users")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    // Solo administradores crean usuarios: el rol que se asigna es una decisión de seguridad.
    [HttpPost]
    [Authorize(Policy = Permissions.UsersManage)]
    [ProducesResponseType<UserDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailsResponse>> Create(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, user);
    }
}
