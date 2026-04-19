using Lynkly.Shared.Kernel.Caching.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Infrastructure.Caching.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverCaching(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKernelCaching();

        return services;
    }
}
