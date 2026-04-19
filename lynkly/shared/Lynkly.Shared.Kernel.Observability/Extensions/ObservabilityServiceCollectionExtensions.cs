using Lynkly.Shared.Kernel.Observability.Correlation;
using Lynkly.Shared.Kernel.Observability.Metrics;
using Lynkly.Shared.Kernel.Observability.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Shared.Kernel.Observability.Extensions;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<ITracingService, NoOpTracingService>();
        services.TryAddSingleton<IMetricsService, NoOpMetricsService>();
        services.TryAddSingleton<ICorrelationService, NoOpCorrelationService>();

        return services;
    }
}
