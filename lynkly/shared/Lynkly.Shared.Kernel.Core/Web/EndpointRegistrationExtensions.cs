using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lynkly.Shared.Kernel.Core.Web;

/// <summary>
/// Provides extension methods for dynamically registering and mapping API endpoints
/// that implement the <see cref="IEndpoint"/> interface.
/// </summary>
public static class EndpointRegistrationExtensions
{
    /// <summary>
    /// Discovers all loaded <see cref="IEndpoint"/> implementations,
    /// creates them through DI-aware activation, and maps their routes.
    /// </summary>
    /// <param name="app">The endpoint route builder used to define API routes.</param>
    public static void MapDiscoveredEndpoints(this IEndpointRouteBuilder app)
    {
        var environment = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var currentScope = EndpointScopeResolver.Resolve(environment.EnvironmentName);

        var endpointTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(static assembly => assembly.ExportedTypes)
            .Where(type => typeof(IEndpoint).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
            .Where(type => IsEndpointEnabledForScope(type, currentScope))
            .OrderBy(type => type.FullName)
            .ToArray();

        using var scope = app.ServiceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        foreach (var endpointType in endpointTypes)
        {
            var endpoint = (IEndpoint)ActivatorUtilities.CreateInstance(serviceProvider, endpointType);
            endpoint.MapEndpoints(app);
        }
    }

    private static bool IsEndpointEnabledForScope(Type endpointType, EndpointScope currentScope)
    {
        var endpointScope = endpointType.GetCustomAttribute<EndpointScopeAttribute>();
        return endpointScope is null || endpointScope.Includes(currentScope);
    }
}