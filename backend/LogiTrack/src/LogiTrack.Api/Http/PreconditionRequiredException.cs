namespace LogiTrack.Api.Http;

// 428: la operación modifica un recurso sin enviar If-Match, así que no se puede comprobar la versión.
public sealed class PreconditionRequiredException(string message) : Exception(message);
