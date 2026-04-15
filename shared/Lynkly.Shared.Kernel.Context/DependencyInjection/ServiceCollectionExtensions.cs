using Lynkly.Shared.Kernel.Context.Abstractions;
using Lynkly.Shared.Kernel.Context.Enrichers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Shared.Kernel.Context.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRequestContextEnricher, CorrelationIdRequestContextEnricher>());

        return services;
    }
}
