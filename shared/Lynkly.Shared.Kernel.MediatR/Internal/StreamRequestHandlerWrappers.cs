using System.Collections.Concurrent;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.MediatR.Internal;

internal abstract class StreamRequestHandlerWrapper
{
    public abstract IAsyncEnumerable<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class StreamRequestHandlerWrapper<TRequest, TResponse> : StreamRequestHandlerWrapper
    where TRequest : IStreamRequest<TResponse>
{
    public override IAsyncEnumerable<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
        return StreamWrap(handler.Handle((TRequest)request, cancellationToken), cancellationToken);
    }

    private static async IAsyncEnumerable<object?> StreamWrap(
        IAsyncEnumerable<TResponse> source,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }
}

internal static class StreamRequestHandlerWrapperCache
{
    private static readonly ConcurrentDictionary<Type, StreamRequestHandlerWrapper> Wrappers = new();

    public static StreamRequestHandlerWrapper GetOrAdd(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        return Wrappers.GetOrAdd(requestType, static type =>
        {
            var requestInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequest<>));

            if (requestInterface is null)
            {
                throw new InvalidOperationException($"Request type '{type.FullName}' does not implement IStreamRequest<TResponse>.");
            }

            var responseType = requestInterface.GetGenericArguments()[0];
            var wrapperType = typeof(StreamRequestHandlerWrapper<,>).MakeGenericType(type, responseType);

            try
            {
                return (StreamRequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to create stream request handler wrapper for type '{type.FullName}'.",
                    exception);
            }
        });
    }
}
