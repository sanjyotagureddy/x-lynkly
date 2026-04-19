using Lynkly.Resolver.Infrastructure.Caching.DependencyInjection;
using Lynkly.Resolver.Infrastructure.Messaging.DependencyInjection;
using Lynkly.Resolver.Infrastructure.Persistence.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Infrastructure.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddResolverInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddResolverPersistence(configuration);
        services.AddResolverCaching();
        services.AddResolverMessaging(configuration);

        return services;
    }
}
