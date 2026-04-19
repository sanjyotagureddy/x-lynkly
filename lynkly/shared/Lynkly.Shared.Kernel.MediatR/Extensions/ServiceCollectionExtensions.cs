using System.Reflection;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Shared.Kernel.MediatR.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Type[] HandlerOpenGenericTypes =
    [
        typeof(IRequestHandler<,>),
        typeof(IRequestHandler<>),
        typeof(INotificationHandler<>),
        typeof(IStreamRequestHandler<,>)
    ];

    public static IServiceCollection AddLynklyMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        services.TryAddTransient<IMediator, Mediator>();
        services.TryAddSingleton<INotificationPublisher, ForeachAwaitNotificationPublisher>();

        RegisterHandlers(services, assemblies);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies.Where(a => a is not null).Distinct())
        {
            foreach (var implementationType in assembly.DefinedTypes.Where(t => t.IsClass && !t.IsAbstract))
            {
                foreach (var serviceType in implementationType.ImplementedInterfaces)
                {
                    if (!serviceType.IsGenericType)
                    {
                        continue;
                    }

                    var openGenericType = serviceType.GetGenericTypeDefinition();
                    if (!HandlerOpenGenericTypes.Contains(openGenericType))
                    {
                        continue;
                    }

                    services.AddTransient(serviceType, implementationType);
                }
            }
        }
    }
}
