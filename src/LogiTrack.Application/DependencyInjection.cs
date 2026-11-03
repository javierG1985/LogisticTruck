using FluentValidation;
using LogiTrack.Application.Common.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LogiTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: los handlers usan IApplicationDbContext, que también es Scoped.
        // El valor por defecto de Mediator (Singleton) capturaría un DbContext de por vida.
        services.AddMediator((MediatorOptions options) =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // El reloj se inyecta para que las pruebas controlen las fechas de creación y modificación.
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
