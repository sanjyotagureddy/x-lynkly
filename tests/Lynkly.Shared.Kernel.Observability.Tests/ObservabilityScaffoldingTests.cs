using Lynkly.Shared.Kernel.Observability.Correlation;
using Lynkly.Shared.Kernel.Observability.Extensions;
using Lynkly.Shared.Kernel.Observability.Metrics;
using Lynkly.Shared.Kernel.Observability.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Observability.Tests;

public class ObservabilityScaffoldingTests
{
    [Fact]
    public void AddObservability_RegistersPlaceholderServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddObservability(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ITracingService>());
        Assert.NotNull(provider.GetService<IMetricsService>());
        Assert.NotNull(provider.GetService<ICorrelationService>());
    }
}
