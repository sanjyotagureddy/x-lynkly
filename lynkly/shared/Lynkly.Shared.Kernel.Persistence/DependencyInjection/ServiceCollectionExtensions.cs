using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
