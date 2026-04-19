using Lynkly.Resolver.Infrastructure.Messaging.Configuration;
using Lynkly.Resolver.Infrastructure.Messaging.DependencyInjection;
using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Messaging.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Lynkly.Resolver.IntegrationTests.Messaging;

public sealed class RabbitMqMessagingRegistrationTests
{
    [Fact]
    public void AddResolverMessaging_Throws_WhenServicesIsNull()
    {
        var configuration = BuildConfiguration();

        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddResolverMessaging(null!, configuration));
    }

    [Fact]
    public void AddResolverMessaging_Throws_WhenConfigurationIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddResolverMessaging(null!));
    }

    [Fact]
    public void AddResolverMessaging_Throws_ForUnsupportedBroker()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(("Messaging:Broker", "999"));

        var exception = Assert.Throws<NotSupportedException>(() => services.AddResolverMessaging(configuration));
        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddResolverMessaging_RegistersPublisherOptionsAndRabbitMqHealthCheck()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddResolverMessaging(configuration);

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        Assert.Equal("MassTransitMessagePublisher", publisher.GetType().Name);

        var messagingOptions = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;
        Assert.Equal(MessagingTransportKind.RabbitMq, messagingOptions.Broker);
        Assert.Equal("lynkly-rabbitmq", messagingOptions.ConnectionStringName);

        var rabbitMqOptions = provider.GetRequiredService<IOptions<RabbitMqMessagingOptions>>().Value;
        Assert.Equal(3, rabbitMqOptions.PublishRetryCount);
        Assert.Equal(TimeSpan.FromSeconds(1), rabbitMqOptions.MinRetryDelay);

        var healthCheckOptions = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
        var rabbitCheck = Assert.Single(healthCheckOptions.Registrations, registration => registration.Name == "lynkly-rabbitmq");
        Assert.Contains("ready", rabbitCheck.Tags);
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] overrides)
    {
        var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Messaging:Broker"] = "RabbitMq",
            ["Messaging:ConnectionStringName"] = "lynkly-rabbitmq",
            ["Messaging:RabbitMq:PublishRetryCount"] = "3",
            ["Messaging:RabbitMq:MinRetryDelay"] = "00:00:01",
            ["Messaging:RabbitMq:MaxRetryDelay"] = "00:00:30",
            ["Messaging:RabbitMq:IntervalDelta"] = "00:00:03",
            ["ConnectionStrings:lynkly-rabbitmq"] = "amqp://guest:guest@localhost:5672"
        };

        foreach (var (key, value) in overrides)
        {
            settings[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
