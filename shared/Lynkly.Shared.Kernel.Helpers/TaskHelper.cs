namespace Lynkly.Shared.Kernel.Helpers;

/// <summary>
/// Provides helper methods for asynchronous and concurrent task execution.
/// </summary>
public static class TaskHelper
{
    /// <summary>
    /// Awaits a task result with timeout.
    /// </summary>
    public static async Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        return await task.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs task factories safely in parallel with bounded concurrency.
    /// </summary>
    public static async Task<IReadOnlyCollection<T>> RunSafelyInParallelAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> taskFactories,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactories);

        if (maxDegreeOfParallelism <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "Degree of parallelism must be greater than zero.");
        }

        var factories = taskFactories as IList<Func<CancellationToken, Task<T>>> ?? taskFactories.ToList();
        if (factories.Count == 0)
        {
            return Array.Empty<T>();
        }

        var results = new T[factories.Count];
        var workerCount = Math.Min(maxDegreeOfParallelism, factories.Count);
        var nextIndex = -1;

        var workers = Enumerable.Range(0, workerCount).Select(_ => Task.Run(async () =>
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var index = Interlocked.Increment(ref nextIndex);
                if (index >= factories.Count)
                {
                    break;
                }

                var factory = factories[index];
                ArgumentNullException.ThrowIfNull(factory);
                results[index] = await factory(cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken));

        await Task.WhenAll(workers).ConfigureAwait(false);
        return results;
    }

    /// <summary>
    /// Executes fire-and-forget task with optional error callback.
    /// </summary>
    public static void FireAndForget(Task task, Action<Exception>? onException = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        _ = task.ContinueWith(
            continuation =>
            {
                if (continuation.Exception is not null)
                {
                    onException?.Invoke(continuation.Exception.GetBaseException());
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }
}
