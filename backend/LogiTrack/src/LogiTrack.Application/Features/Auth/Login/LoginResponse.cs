namespace LogiTrack.Application.Features.Auth.Login;

// El token viaja solo hacia la capa Api, que lo guarda en una cookie HttpOnly. Nunca se devuelve en el cuerpo JSON.
public sealed record LoginResponse(string AccessToken, int ExpiresIn, string Email, string Role);
