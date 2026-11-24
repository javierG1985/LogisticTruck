namespace LogiTrack.Api.Security;

// Cookie de sesión. HttpOnly impide que el JavaScript de la página (y por tanto un XSS) lea el token.
// Secure lo envía solo por HTTPS (los navegadores aceptan Secure en localhost). SameSite=Lax no la envía en peticiones
// que vienen de otro sitio, lo que mitiga CSRF.
public static class AuthCookie
{
    public const string Name = "lt_session";

    public static CookieOptions Create(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = "/",
        Expires = expires,
    };
}
