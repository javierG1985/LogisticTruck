using LogiTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace LogiTrack.Api.IntegrationTests.Infrastructure;

// Levanta la API contra un PostgreSQL con PostGIS real (Testcontainers) y aplica las migraciones al arrancar.
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgis/postgis:18-3.6").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:LogiTrack", _database.GetConnectionString());
        builder.ConfigureServices(services =>
            services.AddScoped<Mediator.INotificationHandler<LogiTrack.Domain.Aggregates.Trucks.Events.TruckStatusChanged>, TruckStatusChangedSpy>());
    }

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
