using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using LogiTrack.Application.Common.Security;
using LogiTrack.Domain.Aggregates.Users;
using LogiTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace LogiTrack.Api.IntegrationTests.Infrastructure;

// Levanta la API contra un PostgreSQL con PostGIS real (Testcontainers), aplica las migraciones (con los roles semilla)
// y siembra usuarios de prueba. Los clientes van por https y, salvo que se pida lo contrario, llevan permisos de Administrator.
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Solo para pruebas: nunca se usa fuera de este proyecto.
    public const string SigningKey = "test-only-logitrack-signing-key-32+chars";
    public const string Issuer = "logitrack-api";
    public const string Audience = "logitrack-clients";

    public const string AdminEmail = "admin@test.local";
    public const string DriverEmail = "driver@test.local";
    public const string SeedPassword = "Seed-password-123";

    // Conjuntos de permisos equivalentes a los roles semilla de la migración.
    public static readonly string[] AdministratorPermissions = Permissions.All;

    public static readonly string[] OperatorPermissions =
        Permissions.All.Where(permission => permission != Permissions.UsersManage).ToArray();

    public static readonly string[] DriverPermissions =
    [
        Permissions.TrucksRead,
        Permissions.DriversRead,
        Permissions.AssignmentsRead,
    ];

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgis/postgis:18-3.6").Build();

    public ApiFactory()
    {
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:LogiTrack", _database.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.ConfigureServices(services =>
            services.AddScoped<Mediator.INotificationHandler<LogiTrack.Domain.Aggregates.Trucks.Events.TruckStatusChanged>, TruckStatusChangedSpy>());
    }

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var now = DateTime.UtcNow;
        db.Users.Add(User.Create(AdminEmail, hasher.Hash(SeedPassword), "Administrator", now));
        db.Users.Add(User.Create(DriverEmail, hasher.Hash(SeedPassword), "Driver", now));
        await db.SaveChangesAsync();
    }

    // Token firmado con la clave de prueba y un claim "permission" por cada permiso indicado.
    public static string CreateToken(IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
        };
        claims.AddRange(permissions.Select(permission => new Claim(Permissions.ClaimType, permission)));

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Cliente con permisos de Administrator por defecto. Oculta el CreateClient de la base (no es virtual).
    public new HttpClient CreateClient()
    {
        var client = base.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(AdministratorPermissions));
        return client;
    }

    // Cliente sin ninguna credencial, para probar los 401.
    public HttpClient CreateAnonymousClient() => base.CreateClient();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
