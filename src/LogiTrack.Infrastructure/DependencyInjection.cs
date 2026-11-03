using EFCore.NamingConventions;
using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogiTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LogiTrack")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:LogiTrack'.");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddHealthChecks().AddNpgSql(connectionString, name: "postgres");

        return services;
    }
}
