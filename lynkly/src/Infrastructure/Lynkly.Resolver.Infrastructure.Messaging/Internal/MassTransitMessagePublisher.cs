using Lynkly.Shared.Kernel.Messaging.Abstractions;
using MassTransit;

namespace Lynkly.Resolver.Infrastructure.Messaging.Internal;

internal sealed class MassTransitMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        return _publishEndpoint.Publish(message, cancellationToken);
    }
}
