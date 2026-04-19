using Lynkly.Shared.Kernel.Observability.Correlation;
using Lynkly.Shared.Kernel.Observability.Configuration;
using Lynkly.Shared.Kernel.Observability.Extensions;
using Lynkly.Shared.Kernel.Observability.Metrics;
using Lynkly.Shared.Kernel.Observability.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Observability.Tests;

public class ObservabilityScaffoldingTests
{
    [Fact]
    public void AddObservability_Throws_WhenServicesAreNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            ObservabilityServiceCollectionExtensions.AddObservability(null!, configuration));
    }

    [Fact]
    public void AddObservability_Throws_WhenConfigurationIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddObservability(null!));
    }

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

    [Fact]
    public void AddObservability_DoesNotOverride_PreRegisteredImplementations()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var tracingService = new TestTracingService();
        var metricsService = new TestMetricsService();
        var correlationService = new TestCorrelationService();

        services.AddSingleton<ITracingService>(tracingService);
        services.AddSingleton<IMetricsService>(metricsService);
        services.AddSingleton<ICorrelationService>(correlationService);

        services.AddObservability(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.Same(tracingService, provider.GetRequiredService<ITracingService>());
        Assert.Same(metricsService, provider.GetRequiredService<IMetricsService>());
        Assert.Same(correlationService, provider.GetRequiredService<ICorrelationService>());
    }

    [Fact]
    public void NoOpTracingService_ReturnsNullActivity()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddObservability(configuration);

        using var provider = services.BuildServiceProvider();
        var tracing = provider.GetRequiredService<ITracingService>();

        Assert.Null(tracing.StartActivity("test"));
    }

    [Fact]
    public void NoOpMetricsService_IncrementCounter_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddObservability(configuration);

        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<IMetricsService>();

        var exception = Record.Exception(() => metrics.IncrementCounter("requests.total", 3));

        Assert.Null(exception);
    }

    [Fact]
    public void NoOpCorrelationService_ReturnsExpectedPlaceholderId()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddObservability(configuration);

        using var provider = services.BuildServiceProvider();
        var correlation = provider.GetRequiredService<ICorrelationService>();

        Assert.Equal("correlation-id-placeholder", correlation.GetOrCreateCorrelationId());
    }

    [Fact]
    public void ObservabilityOptions_ExposeExpectedSectionNameAndValues()
    {
        var options = new ObservabilityOptions
        {
            ServiceName = "lynkly-resolver",
            ServiceVersion = "1.0.0"
        };

        Assert.Equal("Observability", ObservabilityOptions.SectionName);
        Assert.Equal("lynkly-resolver", options.ServiceName);
        Assert.Equal("1.0.0", options.ServiceVersion);
    }

    private sealed class TestTracingService : ITracingService
    {
        public System.Diagnostics.Activity? StartActivity(string name, System.Diagnostics.ActivityKind kind = System.Diagnostics.ActivityKind.Internal) => null;
    }

    private sealed class TestMetricsService : IMetricsService
    {
        public void IncrementCounter(string name, long value = 1)
        {
        }
    }

    private sealed class TestCorrelationService : ICorrelationService
    {
        public string GetOrCreateCorrelationId() => "test-correlation-id";
    }
}
