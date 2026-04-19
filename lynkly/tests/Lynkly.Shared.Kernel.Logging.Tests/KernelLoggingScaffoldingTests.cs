using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.Logging.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace Lynkly.Shared.Kernel.Logging.Tests;

public sealed class KernelLoggingScaffoldingTests
{
    [Fact]
    public void AddKernelLogging_Throws_WhenServicesAreNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddKernelLogging(null!, configuration));
    }

    [Fact]
    public void AddKernelLogging_Throws_WhenConfigurationIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddKernelLogging(null!));
    }

    [Fact]
    public void AddKernelLogging_RegistersStructuredLoggerAbstraction()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddKernelLogging(configuration);

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetService<IStructuredLogger<KernelLoggingScaffoldingTests>>();

        Assert.NotNull(logger);
    }

    [Fact]
    public void StructuredLogger_EmitsNamedProperties()
    {
        var sink = new InMemorySink();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddKernelLogging(configuration, loggerConfiguration => loggerConfiguration.WriteTo.Sink(sink));

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<IStructuredLogger<KernelLoggingScaffoldingTests>>();

        logger.LogInformation(
            "Handled command for UserId {UserId} RequestId {RequestId} EntityId {EntityId}",
            "user-123",
            "req-abc",
            "entity-xyz");

        var logEvent = Assert.Single(sink.Events, e => e.MessageTemplate.Text.Contains("Handled command"));
        Assert.Equal("user-123", logEvent.Properties["UserId"].ToString().Trim('"'));
        Assert.Equal("req-abc", logEvent.Properties["RequestId"].ToString().Trim('"'));
        Assert.Equal("entity-xyz", logEvent.Properties["EntityId"].ToString().Trim('"'));
    }

    private sealed class InMemorySink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
