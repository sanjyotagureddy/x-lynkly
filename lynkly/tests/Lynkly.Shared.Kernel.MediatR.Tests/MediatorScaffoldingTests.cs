using Lynkly.Shared.Kernel.MediatR;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.MediatR.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.MediatR.Tests;

public sealed class MediatorScaffoldingTests
{
    [Fact]
    public async Task Send_ReturnsExpectedResponse()
    {
        var provider = BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new PingRequest("hello"));

        Assert.Equal("HELLO", response);
    }

    [Fact]
    public async Task Send_VoidRequest_InvokesHandler()
    {
        var tracker = new CallTracker();
        var provider = BuildServiceProvider(tracker);
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new VoidPingRequest("side-effect"));

        Assert.Equal("side-effect", tracker.LastMessage);
    }

    [Fact]
    public async Task Publish_InvokesAllNotificationHandlers()
    {
        var tracker = new NotificationTracker();
        var provider = BuildServiceProvider(notificationTracker: tracker);
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Publish(new PingNotification("fanout"));

        Assert.Equal(2, tracker.Count);
    }

    [Fact]
    public async Task CreateStream_ReturnsStreamValues()
    {
        var provider = BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var values = new List<int>();
        await foreach (var item in mediator.CreateStream(new NumberStreamRequest(3)))
        {
            values.Add(item);
        }

        Assert.Equal([1, 2, 3], values);
    }

    [Fact]
    public async Task Send_ThrowsWhenRequestIsNull()
    {
        var provider = BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send<string>(null!));
    }

    [Fact]
    public async Task Publish_ThrowsWhenNotificationIsNull()
    {
        var provider = BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Publish(null!));
    }

    private static ServiceProvider BuildServiceProvider(
        CallTracker? tracker = null,
        NotificationTracker? notificationTracker = null)
    {
        tracker ??= new CallTracker();
        notificationTracker ??= new NotificationTracker();

        var services = new ServiceCollection();
        services.AddSingleton(tracker);
        services.AddSingleton(notificationTracker);
        services.AddLynklyMediator(typeof(MediatorScaffoldingTests).Assembly);

        return services.BuildServiceProvider();
    }

    private sealed record PingRequest(string Message) : IRequest<string>;

    private sealed class PingRequestHandler : IRequestHandler<PingRequest, string>
    {
        public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Message.ToUpperInvariant());
    }

    private sealed record VoidPingRequest(string Message) : IRequest;

    private sealed class VoidPingRequestHandler(CallTracker tracker) : IRequestHandler<VoidPingRequest>
    {
        public Task Handle(VoidPingRequest request, CancellationToken cancellationToken)
        {
            tracker.LastMessage = request.Message;
            return Task.CompletedTask;
        }
    }

    private sealed record PingNotification(string Message) : INotification;

    private sealed class FirstPingNotificationHandler(NotificationTracker tracker) : INotificationHandler<PingNotification>
    {
        public Task Handle(PingNotification notification, CancellationToken cancellationToken)
        {
            tracker.Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class SecondPingNotificationHandler(NotificationTracker tracker) : INotificationHandler<PingNotification>
    {
        public Task Handle(PingNotification notification, CancellationToken cancellationToken)
        {
            tracker.Count++;
            return Task.CompletedTask;
        }
    }

    private sealed record NumberStreamRequest(int Max) : IStreamRequest<int>;

    private sealed class NumberStreamRequestHandler : IStreamRequestHandler<NumberStreamRequest, int>
    {
        public async IAsyncEnumerable<int> Handle(
            NumberStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var i = 1; i <= request.Max; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return i;
            }
        }
    }

    private sealed class CallTracker
    {
        public string? LastMessage { get; set; }
    }

    private sealed class NotificationTracker
    {
        public int Count { get; set; }
    }
}
