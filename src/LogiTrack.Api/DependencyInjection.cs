using Microsoft.Extensions.DependencyInjection;

namespace LogiTrack.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}
