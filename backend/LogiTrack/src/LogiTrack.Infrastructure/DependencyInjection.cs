using EFCore.NamingConventions;
using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Security;
using LogiTrack.Infrastructure.Persistence;
using LogiTrack.Infrastructure.Persistence.Interceptors;
using LogiTrack.Infrastructure.Security;
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

        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<AppDbContext>((provider, options) => options
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(provider.GetRequiredService<DispatchDomainEventsInterceptor>()));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddHealthChecks().AddNpgSql(connectionString, name: "postgres");

        return services;
    }
}
