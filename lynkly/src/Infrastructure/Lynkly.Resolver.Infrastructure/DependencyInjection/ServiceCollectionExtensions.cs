using Lynkly.Resolver.Infrastructure.Caching.DependencyInjection;
using Lynkly.Resolver.Infrastructure.Messaging.DependencyInjection;
using Lynkly.Resolver.Infrastructure.Persistence.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Infrastructure.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddResolverInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddResolverPersistence();
        services.AddResolverCaching();
        services.AddResolverMessaging();

        return services;
    }
}
