using System.Globalization;
using LogiTrack.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LogiTrack.Api.Http;

// Versión del recurso en el encabezado ETag (lectura) y If-Match (escritura).
public static class ETag
{
    public static string FromVersion(uint version) => $"\"{version.ToString(CultureInfo.InvariantCulture)}\"";

    public static uint RequireIfMatch(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("If-Match", out var values) || string.IsNullOrWhiteSpace(values.ToString()))
        {
            throw new PreconditionRequiredException("Falta el encabezado If-Match con la versión del recurso.");
        }

        var raw = values.ToString().Trim().Trim('"');

        if (!uint.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var version))
        {
            throw new PreconditionFailedException("El encabezado If-Match no contiene una versión válida.");
        }

        return version;
    }
}
