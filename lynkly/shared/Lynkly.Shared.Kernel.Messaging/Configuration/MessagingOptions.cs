namespace Lynkly.Shared.Kernel.Messaging.Configuration;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public MessagingTransportKind Broker { get; init; } = MessagingTransportKind.RabbitMq;

    public string ConnectionStringName { get; init; } = "lynkly-rabbitmq";
}
