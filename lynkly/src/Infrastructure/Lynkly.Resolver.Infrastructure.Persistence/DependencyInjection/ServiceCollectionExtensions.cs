using Lynkly.Shared.Kernel.Persistence.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Infrastructure.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKernelPersistence();

        return services;
    }
}
