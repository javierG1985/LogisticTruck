using LogiTrack.Api.Security;
using LogiTrack.Application.Features.Auth.Login;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<SessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionResponse>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var login = await sender.Send(command, cancellationToken);

        Response.Cookies.Append(
            AuthCookie.Name,
            login.AccessToken,
            AuthCookie.Create(DateTimeOffset.UtcNow.AddSeconds(login.ExpiresIn)));

        return Ok(new SessionResponse(login.Email, login.Role, login.ExpiresIn));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookie.Name, AuthCookie.Create(null));
        return NoContent();
    }
}
