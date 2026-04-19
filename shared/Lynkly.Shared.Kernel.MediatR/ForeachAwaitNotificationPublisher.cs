using Lynkly.Shared.Kernel.MediatR.Abstractions;

namespace Lynkly.Shared.Kernel.MediatR;

public sealed class ForeachAwaitNotificationPublisher : INotificationPublisher
{
    public async Task Publish<TNotification>(
        IEnumerable<INotificationHandler<TNotification>> handlers,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
