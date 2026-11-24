using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LogiTrack.Application.Common.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LogiTrack.Api.Security;

public sealed class JwtTokenIssuer(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenIssuer
{
    public IssuedToken Issue(Guid userId, string email, string role, IReadOnlyCollection<string> permissions)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", role),
        };

        // Un claim por permiso: las políticas de la API comprueban estos valores.
        claims.AddRange(permissions.Select(permission => new Claim(Permissions.ClaimType, permission)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(jwt.ExpiresMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), jwt.ExpiresMinutes * 60);
    }
}
