namespace LogiTrack.Api.Controllers.Auth;

// Datos de la sesión iniciada. El token no está aquí: viaja solo en la cookie HttpOnly.
public sealed record SessionResponse(string Email, string Role, int ExpiresIn);
