using Lynkly.Shared.Kernel.Messaging.Abstractions;

namespace Lynkly.Shared.Kernel.Messaging.Internal;

internal sealed class UnconfiguredMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        throw new InvalidOperationException(
            "No messaging transport is configured. Register a broker implementation in infrastructure.");
    }
}
