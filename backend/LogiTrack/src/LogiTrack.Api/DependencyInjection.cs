using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using LogiTrack.Api.Http;
using LogiTrack.Api.Security;
using LogiTrack.Application.Common.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LogiTrack.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddOpenApi();
        services.AddHealthChecks();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddJwtAuthentication(configuration, environment);

        return services;
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Si falta la clave o es corta, la aplicación no arranca: mejor fallar al inicio que aceptar tokens débiles.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                var options = jwt.Value;

                // HTTPS se exige fuera de desarrollo. En desarrollo se permite http para facilitar las pruebas locales.
                bearer.RequireHttpsMetadata = !environment.IsDevelopment();
                // Sin mapeo de claims: el rol viaja como claim "role" tal cual y "sub" es el identificador.
                bearer.MapInboundClaims = false;

                // El token llega en la cookie HttpOnly. Si la petición trae la cabecera Authorization (clientes de API y pruebas), esa manda.
                bearer.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token)
                            && context.Request.Cookies.TryGetValue(AuthCookie.Name, out var cookie))
                        {
                            context.Token = cookie;
                        }

                        return Task.CompletedTask;
                    },
                };
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidAlgorithms = [Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256],
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role",
                };
            });

        services.AddAuthorizationBuilder()
            // Todo endpoint exige autenticación salvo que diga lo contrario con [AllowAnonymous].
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        // Una política por permiso: el token lleva un claim "permission" por cada permiso del rol.
        foreach (var permission in Permissions.All)
        {
            services.AddAuthorizationBuilder().AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
        }
    }
}
