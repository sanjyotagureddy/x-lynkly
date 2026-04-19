using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using MassTransit;

namespace Lynkly.Resolver.Infrastructure.Messaging.Internal;

internal sealed class MassTransitMessagePublisher(
    IPublishEndpoint publishEndpoint,
    IStructuredLogger<MassTransitMessagePublisher> logger) : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    private readonly IStructuredLogger<MassTransitMessagePublisher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = typeof(TMessage).Name;
        _logger.LogInformation(
            "Publishing integration event MessageType {MessageType}",
            messageType);

        try
        {
            await _publishEndpoint.Publish(message, cancellationToken);
            _logger.LogInformation(
                "Published integration event MessageType {MessageType}",
                messageType);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish integration event MessageType {MessageType}",
                messageType);
            throw;
        }
    }

}
