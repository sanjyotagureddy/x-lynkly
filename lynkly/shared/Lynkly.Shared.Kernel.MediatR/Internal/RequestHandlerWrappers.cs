using System.Collections.Concurrent;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.MediatR.Internal;

internal abstract class RequestHandlerWrapper
{
    public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.Handle((TRequest)request, cancellationToken).ConfigureAwait(false);
    }
}

internal static class RequestHandlerWrapperCache
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> Wrappers = new();

    public static RequestHandlerWrapper GetOrAdd(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        return Wrappers.GetOrAdd(requestType, static type =>
        {
            var requestInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            if (requestInterface is null)
            {
                throw new InvalidOperationException($"Request type '{type.FullName}' does not implement IRequest<TResponse>.");
            }

            var responseType = requestInterface.GetGenericArguments()[0];
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(type, responseType);

            try
            {
                return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to create request handler wrapper for type '{type.FullName}'.",
                    exception);
            }
        });
    }
}
