using LogiTrack.Application.Common.Exceptions;

namespace LogiTrack.Application.Common.Concurrency;

// Compara la versión que el cliente envió en If-Match con la actual del recurso. Si no coincide, alguien cambió el recurso antes.
internal static class VersionCheck
{
    public static void EnsureMatches(uint currentVersion, uint expectedVersion, string resource)
    {
        if (currentVersion != expectedVersion)
        {
            throw new PreconditionFailedException($"{resource} cambió desde la última lectura. Vuelve a leerlo y reintenta.");
        }
    }
}
