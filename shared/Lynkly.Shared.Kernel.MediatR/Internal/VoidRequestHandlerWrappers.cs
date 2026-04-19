using System.Collections.Concurrent;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.MediatR.Internal;

internal abstract class VoidRequestHandlerWrapper
{
    public abstract Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class VoidRequestHandlerWrapper<TRequest> : VoidRequestHandlerWrapper
    where TRequest : IRequest
{
    public override Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        return handler.Handle((TRequest)request, cancellationToken);
    }
}

internal static class VoidRequestHandlerWrapperCache
{
    private static readonly ConcurrentDictionary<Type, VoidRequestHandlerWrapper> Wrappers = new();

    public static VoidRequestHandlerWrapper GetOrAdd(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        return Wrappers.GetOrAdd(requestType, static type =>
        {
            if (!typeof(IRequest).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Request type '{type.FullName}' does not implement IRequest.");
            }

            var wrapperType = typeof(VoidRequestHandlerWrapper<>).MakeGenericType(type);

            try
            {
                return (VoidRequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to create void request handler wrapper for type '{type.FullName}'.",
                    exception);
            }
        });
    }
}
