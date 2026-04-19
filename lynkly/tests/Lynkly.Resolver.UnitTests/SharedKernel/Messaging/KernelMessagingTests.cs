using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Messaging.Configuration;
using Lynkly.Shared.Kernel.Messaging.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Messaging;

public sealed class KernelMessagingTests
{
    [Fact]
    public void AddKernelMessaging_Throws_WhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddKernelMessaging(null!));
    }

    [Fact]
    public async Task AddKernelMessaging_RegistersFallbackPublisherThatThrowsWhenUsed()
    {
        var services = new ServiceCollection();
        services.AddKernelMessaging();

        await using var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IMessagePublisher>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            publisher.PublishAsync(new TestMessage("hello")));
        Assert.Contains("No messaging transport is configured", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MessagingOptions_HaveExpectedDefaults()
    {
        var options = new MessagingOptions();

        Assert.Equal("Messaging", MessagingOptions.SectionName);
        Assert.Equal(MessagingTransportKind.RabbitMq, options.Broker);
        Assert.Equal("lynkly-rabbitmq", options.ConnectionStringName);
    }

    private sealed record TestMessage(string Value);
}
