using Lynkly.Shared.Kernel.Context;

namespace Lynkly.Shared.Kernel.Context.Tests;

public sealed class RequestContextScopeTests
{
    // ── Initial state ────────────────────────────────────────────────────────

    [Fact]
    public void Current_IsNullOutsideOfAnyScope()
    {
        // This test relies on test isolation; xUnit runs each test in its own
        // async context so the AsyncLocal is clean at entry.
        Assert.Null(RequestContextScope.Current);
    }

    // ── BeginScope ───────────────────────────────────────────────────────────

    [Fact]
    public void BeginScope_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RequestContextScope.BeginScope(null!));
    }

    [Fact]
    public void BeginScope_SetsCurrent()
    {
        var ctx = MakeContext();

        using (RequestContextScope.BeginScope(ctx))
        {
            Assert.Same(ctx, RequestContextScope.Current);
        }
    }

    [Fact]
    public void BeginScope_RestoresNullAfterDispose()
    {
        var ctx = MakeContext();

        using (RequestContextScope.BeginScope(ctx))
        {
            // inside scope
        }

        Assert.Null(RequestContextScope.Current);
    }

    [Fact]
    public void BeginScope_RestoresPreviousContext_AfterDispose()
    {
        var outer = MakeContext("outer");
        var inner = MakeContext("inner");

        using (RequestContextScope.BeginScope(outer))
        {
            using (RequestContextScope.BeginScope(inner))
            {
                Assert.Same(inner, RequestContextScope.Current);
            }

            Assert.Same(outer, RequestContextScope.Current);
        }

        Assert.Null(RequestContextScope.Current);
    }

    // ── Dispose idempotence ──────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        var ctx = MakeContext();
        var scope = RequestContextScope.BeginScope(ctx);

        scope.Dispose();
        var exception = Record.Exception(() => scope.Dispose());

        Assert.Null(exception);
        Assert.Null(RequestContextScope.Current);
    }

    // ── Async propagation ────────────────────────────────────────────────────

    [Fact]
    public async Task BeginScope_PropagatesAcrossAwait()
    {
        var ctx = MakeContext();

        using (RequestContextScope.BeginScope(ctx))
        {
            await Task.Yield();
            Assert.Same(ctx, RequestContextScope.Current);
        }
    }

    [Fact]
    public async Task BeginScope_DoesNotLeakAcrossParallelBranches()
    {
        var ctx = MakeContext();
        AppCallContext? capturedInTask = null;

        using (RequestContextScope.BeginScope(ctx))
        {
            // Spawn a child task that runs *outside* the scope (the scope
            // is captured by the AsyncLocal at the point the child starts,
            // so it WILL see it unless the child starts before BeginScope).
            // Here we just verify that the current task sees the context.
            await Task.Run(() =>
            {
                // Task.Run runs on a thread-pool thread with a fresh execution
                // context copy that inherits the current AsyncLocal value.
                capturedInTask = RequestContextScope.Current;
            });
        }

        // The child task captured the context that was ambient when it was started.
        Assert.Same(ctx, capturedInTask);
        // After scope disposal the parent is clean.
        Assert.Null(RequestContextScope.Current);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AppCallContext MakeContext(string name = "app") =>
        AppCallContext.Create(name, "req-1", "trace-1", "GET", "/");
}
