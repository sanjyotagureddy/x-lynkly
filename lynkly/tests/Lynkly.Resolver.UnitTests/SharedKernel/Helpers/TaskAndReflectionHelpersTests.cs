using System.Reflection;

using Lynkly.Shared.Kernel.Core.Helpers;
using Lynkly.Shared.Kernel.Core.Helpers.Reflection;
using Lynkly.Shared.Kernel.Core.Helpers.Threading;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class TaskAndReflectionHelpersTests
{
    private sealed class Source
    {
        public string Name { get; init; } = string.Empty;
        public int Count { get; init; }
        public string Skip { get; init; } = string.Empty;
    }

    private sealed class Target
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Skip { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    private sealed class SampleAttribute : Attribute
    {
    }

    private sealed class ReflectionSample
    {
        [Sample]
        public string Name { get; init; } = "lynkly";

        public string? Optional { get; init; }

        private string Secret => "hidden";
    }

    private sealed class ReflectionSampleWithIndexer
    {
        public string Name { get; init; } = "lynkly";

        public string this[int index] => index.ToString();
    }

    [Fact]
    public async Task TaskHelper_WithTimeoutAsync_Should_Work()
    {
        var success = await TaskHelper.WithTimeoutAsync(Task.FromResult(42), TimeSpan.FromSeconds(1));
        Assert.Equal(42, success);

        await Assert.ThrowsAsync<TimeoutException>(() => TaskHelper.WithTimeoutAsync(
            Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => 1),
            TimeSpan.FromMilliseconds(1)));

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => TaskHelper.WithTimeoutAsync(
            Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => 1),
            TimeSpan.FromSeconds(1),
            cts.Token));

        await Assert.ThrowsAsync<ArgumentNullException>(() => TaskHelper.WithTimeoutAsync<int>(null!, TimeSpan.FromSeconds(1)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TaskHelper.WithTimeoutAsync(Task.FromResult(1), TimeSpan.Zero));
    }

    [Fact]
    public async Task TaskHelper_RunSafelyInParallelAsync_Should_Work()
    {
        var factories = new List<Func<CancellationToken, Task<int>>>
        {
            _ => Task.FromResult(1),
            _ => Task.FromResult(2),
            _ => Task.FromResult(3)
        };

        var result = await TaskHelper.RunSafelyInParallelAsync(factories, 2);

        Assert.Equal(3, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
        Assert.Contains(3, result);

        await Assert.ThrowsAsync<ArgumentNullException>(() => TaskHelper.RunSafelyInParallelAsync<int>(null!, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TaskHelper.RunSafelyInParallelAsync(factories, 0));

        var invalidFactories = new Func<CancellationToken, Task<int>>[] { null! };
        await Assert.ThrowsAsync<ArgumentNullException>(() => TaskHelper.RunSafelyInParallelAsync(invalidFactories, 1));
    }

    [Fact]
    public async Task TaskHelper_FireAndForget_Should_ReportFailures()
    {
        var tcs = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskHelper.FireAndForget(Task.FromException(new InvalidOperationException("failed")), ex => tcs.TrySetResult(ex));

        var exception = await TaskHelper.WithTimeoutAsync(tcs.Task, TimeSpan.FromSeconds(1));
        Assert.IsType<InvalidOperationException>(exception);

        Assert.Throws<ArgumentNullException>(() => TaskHelper.FireAndForget(null!));
    }

    [Fact]
    public void ReflectionHelper_Should_Work()
    {
        var sample = new ReflectionSample();

        Assert.Equal("lynkly", ReflectionHelper.GetPropertyValue<string>(sample, "Name"));
        Assert.Equal("hidden", ReflectionHelper.GetPropertyValue<string>(sample, "Secret"));
        Assert.Null(ReflectionHelper.GetPropertyValue<string>(sample, "Optional"));
        Assert.Throws<InvalidOperationException>(() => ReflectionHelper.GetPropertyValue<string>(sample, "Missing"));
        Assert.Throws<InvalidCastException>(() => ReflectionHelper.GetPropertyValue<int>(sample, "Name"));
        Assert.Throws<ArgumentNullException>(() => ReflectionHelper.GetPropertyValue<string>(null!, "Name"));
        Assert.Throws<ArgumentException>(() => ReflectionHelper.GetPropertyValue<string>(sample, " "));

        var attribute = ReflectionHelper.GetAttribute<SampleAttribute>(typeof(ReflectionSample).GetProperty(nameof(ReflectionSample.Name), BindingFlags.Public | BindingFlags.Instance)!);
        Assert.NotNull(attribute);
        Assert.Null(ReflectionHelper.GetAttribute<ObsoleteAttribute>(typeof(ReflectionSample).GetProperty(nameof(ReflectionSample.Name), BindingFlags.Public | BindingFlags.Instance)!));
        Assert.Throws<ArgumentNullException>(() => ReflectionHelper.GetAttribute<SampleAttribute>(null!));

        var mapped = ReflectionHelper.MapProperties<Source, Target>(new Source { Name = "x", Count = 5, Skip = "not assignable" });
        Assert.Equal("x", mapped.Name);
        Assert.Equal(5, mapped.Count);
        Assert.Equal(0, mapped.Skip);

        var indexedMapped = ReflectionHelper.MapProperties<ReflectionSampleWithIndexer, Target>(new ReflectionSampleWithIndexer());
        Assert.Equal("lynkly", indexedMapped.Name);

        Assert.Throws<ArgumentNullException>(() => ReflectionHelper.MapProperties<Source, Target>(null!));
    }
}
