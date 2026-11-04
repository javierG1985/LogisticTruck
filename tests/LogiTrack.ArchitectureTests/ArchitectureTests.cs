using System.Reflection;
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
    public void Modules_CrossReferencesInAnyLayer_AreForbidden()
    {
        var modules = Layers
            .SelectMany(layer => layer.Assembly.GetTypes())
            .Select(type => type.Namespace)
            .OfType<string>()
            .Where(ns => ns.Contains(".Modules.", StringComparison.Ordinal))
            .Select(ns => ns.Split(".Modules.")[1].Split('.')[0])
            .Distinct()
            .ToList();

        foreach (var (layer, assembly) in Layers)
        {
            foreach (var module in modules)
            {
                var otherModuleNamespaces = modules
                    .Where(other => other != module)
                    .SelectMany(other => Layers.Select(l => $"LogiTrack.{l.Layer}.Modules.{other}"))
                    .ToArray();

                if (otherModuleNamespaces.Length == 0)
                {
                    continue;
                }

                var result = Types.InAssembly(assembly)
                    .That().ResideInNamespaceStartingWith($"LogiTrack.{layer}.Modules.{module}")
                    .ShouldNot().HaveDependencyOnAny(otherModuleNamespaces)
                    .GetResult();

                Assert.True(result.IsSuccessful, FailureMessage(result.FailingTypeNames));
            }
        }
    }

    [Fact]
    public void Controllers_WithOnlySenderAndOwnResource_AreAllowed()
    {
        var controllers = ApiAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Controller", StringComparison.Ordinal)
                && type.Namespace?.StartsWith("LogiTrack.Api.Controllers.", StringComparison.Ordinal) == true)
            .ToList();

        foreach (var controller in controllers)
        {
            var resource = controller.Namespace![("LogiTrack.Api.Controllers.").Length..];
            var allowedNamespaces = new[]
            {
                $"LogiTrack.Api.Controllers.{resource}",
                $"LogiTrack.Api.Requests.{resource}",
                $"LogiTrack.Application.Modules.{resource}",
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
