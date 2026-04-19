namespace Lynkly.Resolver.Infrastructure.Messaging.Configuration;

public sealed class RabbitMqMessagingOptions
{
    public const string SectionName = "Messaging:RabbitMq";

    public int PublishRetryCount { get; init; } = 3;

    public TimeSpan MinRetryDelay { get; init; } = TimeSpan.FromSeconds(1);

    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);

    public TimeSpan IntervalDelta { get; init; } = TimeSpan.FromSeconds(3);
}
