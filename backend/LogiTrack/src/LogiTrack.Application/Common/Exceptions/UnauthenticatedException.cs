namespace LogiTrack.Application.Common.Exceptions;

// Credenciales no válidas. El mensaje es genérico a propósito: no revela si fue el correo o la contraseña.
public sealed class UnauthenticatedException(string message) : Exception(message);
