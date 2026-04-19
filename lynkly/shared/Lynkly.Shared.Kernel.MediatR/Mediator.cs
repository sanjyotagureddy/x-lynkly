using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.MediatR.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.MediatR;

public sealed class Mediator(IServiceProvider serviceProvider, INotificationPublisher notificationPublisher) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly INotificationPublisher _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = RequestHandlerWrapperCache.GetOrAdd(request.GetType());
        var response = await wrapper.Handle(request, _serviceProvider, cancellationToken).ConfigureAwait(false);
        return (TResponse)response!;
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = VoidRequestHandlerWrapperCache.GetOrAdd(request.GetType());
        return wrapper.Handle(request, _serviceProvider, cancellationToken);
    }

    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var wrapper = NotificationHandlerWrapperCache.GetOrAdd(notification.GetType());
        return wrapper.Handle(notification, _serviceProvider, _notificationPublisher, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = StreamRequestHandlerWrapperCache.GetOrAdd(request.GetType());
        var stream = wrapper.Handle(request, _serviceProvider, cancellationToken);
        return StreamCast<TResponse>(stream, cancellationToken);
    }

    private static async IAsyncEnumerable<TResponse> StreamCast<TResponse>(
        IAsyncEnumerable<object?> stream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return (TResponse)item!;
        }
    }
}
