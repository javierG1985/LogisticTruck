using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Api.Security;

// Sección "Jwt" de la configuración. La clave de firma no se versiona: en producción llega por variable de entorno (Jwt__SigningKey).
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    // HS256 exige al menos 256 bits: 32 caracteres ASCII.
    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiresMinutes { get; init; } = 60;
}
