using Lynkly.Shared.Kernel.Messaging.DependencyInjection;
using Lynkly.Shared.Kernel.Persistence.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Infrastructure.Messaging.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverMessaging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKernelMessaging();
        services.AddKernelPersistence();

        return services;
    }
}
