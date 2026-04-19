using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Messaging.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelMessaging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
