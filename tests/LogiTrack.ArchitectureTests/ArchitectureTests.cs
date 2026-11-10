using System.Reflection;
using LogiTrack.Domain.SharedKernel;
using Mediator;
using NetArchTest.Rules;

namespace LogiTrack.ArchitectureTests;

public sealed class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(LogiTrack.Domain.SharedKernel.Entity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(LogiTrack.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(LogiTrack.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(LogiTrack.Api.DependencyInjection).Assembly;

    private static readonly (string Layer, Assembly Assembly)[] Layers =
    [
        ("Domain", DomainAssembly),
        ("Application", ApplicationAssembly),
        ("Infrastructure", InfrastructureAssembly),
        ("Api", ApiAssembly),
    ];

    [Fact]
    public void DomainLayer_WithoutForbiddenDependencies_IsSuccessful()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespaceStartingWith("LogiTrack.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Npgsql",
                "LogiTrack.Application",
                "LogiTrack.Infrastructure",
                "LogiTrack.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void ApplicationLayer_WithoutNpgsqlOrAspNetOrInfrastructure_IsSuccessful()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespaceStartingWith("LogiTrack.Application")
            .ShouldNot().HaveDependencyOnAny(
                "Npgsql",
                "Microsoft.AspNetCore",
                "LogiTrack.Infrastructure",
                "LogiTrack.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Aggregates_CrossReferencesInAnyLayer_AreForbidden()
    {
        AssertNoCrossReferences("Aggregates");
    }

    [Fact]
    public void Features_CrossReferencesInAnyLayer_AreForbidden()
    {
        AssertNoCrossReferences("Features");
    }

    [Fact]
    public void Commands_ImplementICommand_AreRequired()
    {
        var commands = ApplicationAssembly.GetTypes()
            .Where(type => IsInFolder(type, "Commands") && type.Name.EndsWith("Command", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(commands);

        foreach (var command in commands)
        {
            var implementsCommand = command.GetInterfaces().Any(i =>
                i == typeof(ICommand)
                || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));

            Assert.True(implementsCommand, $"{command.FullName} está en Commands/ pero no implementa ICommand.");
        }
    }

    [Fact]
    public void Queries_ImplementIQuery_AreRequired()
    {
        var queries = ApplicationAssembly.GetTypes()
            .Where(type => IsInFolder(type, "Queries") && type.Name.EndsWith("Query", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(queries);

        foreach (var query in queries)
        {
            var implementsQuery = query.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

            Assert.True(implementsQuery, $"{query.FullName} está en Queries/ pero no implementa IQuery<T>.");
        }
    }

    [Fact]
    public void Queries_DoNotContainCommands_AndCommandsDoNotContainQueries()
    {
        var misplaced = ApplicationAssembly.GetTypes()
            .Where(type => (IsInFolder(type, "Queries") && type.Name.EndsWith("Command", StringComparison.Ordinal))
                || (IsInFolder(type, "Commands") && type.Name.EndsWith("Query", StringComparison.Ordinal)))
            .Select(type => type.FullName)
            .ToList();

        Assert.Empty(misplaced);
    }

    [Fact]
    public void Controllers_WithOnlySenderAndOwnFeature_AreAllowed()
    {
        var controllers = ApiAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Controller", StringComparison.Ordinal)
                && type.Namespace?.StartsWith("LogiTrack.Api.Controllers.", StringComparison.Ordinal) == true)
            .ToList();

        Assert.NotEmpty(controllers);

        foreach (var controller in controllers)
        {
            var resource = controller.Namespace![("LogiTrack.Api.Controllers.").Length..];
            var allowedNamespaces = new[]
            {
                $"LogiTrack.Api.Controllers.{resource}",
                $"LogiTrack.Application.Features.{resource}",
                "LogiTrack.Application.Common.Pagination",
            };

            var constructorTypes = controller.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Select(parameter => parameter.ParameterType);

            Assert.All(constructorTypes, type => Assert.Equal(typeof(ISender), type));

            var memberTypes = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType))
                .SelectMany(Unwrap)
                .Where(type => type.Namespace?.StartsWith("LogiTrack.", StringComparison.Ordinal) == true);

            foreach (var type in memberTypes)
            {
                Assert.True(
                    allowedNamespaces.Any(prefix => type.Namespace!.StartsWith(prefix, StringComparison.Ordinal)),
                    $"{controller.Name} usa {type.FullName}, que no pertenece a su recurso.");
            }
        }
    }

    [Fact]
    public void DomainEntities_WithPublicSetters_AreForbidden()
    {
        var entities = DomainAssembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(LogiTrack.Domain.SharedKernel.Entity)));

        foreach (var entity in entities)
        {
            var publicSetters = entity
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.SetMethod is { IsPublic: true })
                .Select(property => property.Name)
                .ToList();

            Assert.True(publicSetters.Count == 0, $"{entity.Name} tiene setters públicos: {string.Join(", ", publicSetters)}");
        }
    }

    // Cada evento de dominio debe tener al menos un handler: Mediator no compila un evento sin él, y esta prueba
    // impide que un handler se borre sin que se note hasta el despacho en producción.
    [Fact]
    public void DomainEvents_HaveAtLeastOneHandler()
    {
        var events = DomainAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IDomainEvent).IsAssignableFrom(type))
            .ToList();

        Assert.NotEmpty(events);

        var handledEvents = ApplicationAssembly.GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var unhandled = events.Where(domainEvent => !handledEvents.Contains(domainEvent)).Select(domainEvent => domainEvent.FullName);

        Assert.Empty(unhandled);
    }

    [Fact]
    public void EventHandlers_ImplementINotificationHandler_AreRequired()
    {
        // El espacio de nombres termina en EventHandlers (no hay nada detrás), así que IsInFolder no sirve aquí.
        var handlers = ApplicationAssembly.GetTypes()
            .Where(type => type.Namespace?.EndsWith(".EventHandlers", StringComparison.Ordinal) == true)
            // El compilador genera clases auxiliares para las lambdas (nombres con "<"); no son handlers.
            .Where(type => type.IsClass && !type.Name.Contains('<'))
            .ToList();

        Assert.NotEmpty(handlers);

        foreach (var handler in handlers)
        {
            var implementsHandler = handler.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

            Assert.True(implementsHandler, $"{handler.FullName} está en EventHandlers/ pero no implementa INotificationHandler<T>.");
        }
    }

    // Cada agregado debe tener su configuración EF: sin ella, la tabla no se crea y el cambio falla en tiempo de ejecución.
    [Fact]
    public void Aggregates_HaveEntityTypeConfiguration()
    {
        var aggregates = DomainAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(AggregateRoot).IsAssignableFrom(type))
            .ToList();

        Assert.NotEmpty(aggregates);

        var configured = InfrastructureAssembly.GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().FullName == EntityTypeConfigurationName)
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var missing = aggregates.Where(aggregate => !configured.Contains(aggregate)).Select(aggregate => aggregate.FullName);

        Assert.Empty(missing);
    }

    // Nombre completo del tipo de EF Core. Se compara por nombre para no añadir EF Core a este proyecto de pruebas.
    private const string EntityTypeConfigurationName = "Microsoft.EntityFrameworkCore.IEntityTypeConfiguration`1";

    // Única excepción a la regla de agregados: la política que combina camión y conductor (ver ADR 0003).
    private const string CrossAggregatePolicy = "TruckAssignmentPolicy";

    // Un agregado o una feature no puede depender de otro del mismo tipo: se comunican con eventos.
    private static void AssertNoCrossReferences(string marker)
    {
        var names = Layers
            .SelectMany(layer => layer.Assembly.GetTypes())
            .Select(type => type.Namespace)
            .OfType<string>()
            .Where(ns => ns.Contains($".{marker}.", StringComparison.Ordinal))
            .Select(ns => ns.Split($".{marker}.")[1].Split('.')[0])
            .Distinct()
            .ToList();

        foreach (var (layer, assembly) in Layers)
        {
            foreach (var name in names)
            {
                var otherNamespaces = names
                    .Where(other => other != name)
                    .SelectMany(other => Layers.Select(l => $"LogiTrack.{l.Layer}.{marker}.{other}"))
                    .ToArray();

                if (otherNamespaces.Length == 0)
                {
                    continue;
                }

                // TruckAssignmentPolicy es la única excepción: es la regla que combina camión y conductor, y vive en el agregado de asignaciones.
                var result = Types.InAssembly(assembly)
                    .That().ResideInNamespaceStartingWith($"LogiTrack.{layer}.{marker}.{name}")
                    .And().DoNotHaveName(CrossAggregatePolicy)
                    .ShouldNot().HaveDependencyOnAny(otherNamespaces)
                    .GetResult();

                Assert.True(result.IsSuccessful, FailureMessage(result.FailingTypeNames));
            }
        }
    }

    private static bool IsInFolder(Type type, string folder) =>
        type.Namespace?.Contains($".{folder}.", StringComparison.Ordinal) == true;

    private static IEnumerable<Type> Unwrap(Type type)
    {
        yield return type;

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (var argument in type.GetGenericArguments())
        {
            foreach (var inner in Unwrap(argument))
            {
                yield return inner;
            }
        }
    }

    private static string FailureMessage(IEnumerable<string>? failingTypes) =>
        "Tipos que incumplen la regla: " + string.Join(", ", failingTypes ?? []);
}
