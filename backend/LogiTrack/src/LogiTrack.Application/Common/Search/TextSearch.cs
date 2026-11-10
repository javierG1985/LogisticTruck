namespace LogiTrack.Application.Common.Search;

// Piezas comunes de la búsqueda de texto. Son provider-neutral: Application no conoce Npgsql.
public static class TextSearch
{
    // Carácter de escape usado en los patrones LIKE generados por ToContainsPattern.
    public const string EscapeCharacter = "\\";

    // Minúsculas dentro de una consulta. EF Core no traduce ToLowerInvariant() a SQL; por eso este método es un
    // marcador: AppDbContext (Infrastructure) lo asocia con lower() de PostgreSQL. Fuera de una consulta funciona en memoria.
    public static string ToLowerText(string value) => value.ToLowerInvariant();

    // Convierte el texto del usuario en un patrón LIKE de tipo "contiene", sin mayúsculas y con % y _ tratados como texto.
    public static string ToContainsPattern(string search)
    {
        var escaped = search.ToLowerInvariant()
            .Replace(EscapeCharacter, EscapeCharacter + EscapeCharacter, StringComparison.Ordinal)
            .Replace("%", EscapeCharacter + "%", StringComparison.Ordinal)
            .Replace("_", EscapeCharacter + "_", StringComparison.Ordinal);

        return $"%{escaped}%";
    }
}
