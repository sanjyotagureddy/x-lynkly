using System.Collections.Concurrent;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.MediatR.Internal;

internal abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(
        INotification notification,
        IServiceProvider serviceProvider,
        INotificationPublisher notificationPublisher,
        CancellationToken cancellationToken);
}

internal sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override Task Handle(
        INotification notification,
        IServiceProvider serviceProvider,
        INotificationPublisher notificationPublisher,
        CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
        return notificationPublisher.Publish(handlers, (TNotification)notification, cancellationToken);
    }
}

internal static class NotificationHandlerWrapperCache
{
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> Wrappers = new();

    public static NotificationHandlerWrapper GetOrAdd(Type notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

        return Wrappers.GetOrAdd(notificationType, static type =>
        {
            if (!typeof(INotification).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Notification type '{type.FullName}' does not implement INotification.");
            }

            var wrapperType = typeof(NotificationHandlerWrapper<>).MakeGenericType(type);

            try
            {
                return (NotificationHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to create notification handler wrapper for type '{type.FullName}'.",
                    exception);
            }
        });
    }
}
