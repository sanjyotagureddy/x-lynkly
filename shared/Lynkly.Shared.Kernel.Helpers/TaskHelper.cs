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

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var delayTask = Task.Delay(timeout, linkedCts.Token);
        var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);

        if (completedTask == delayTask)
        {
            throw new TimeoutException($"Task execution exceeded timeout of {timeout}.");
        }

        return await task.ConfigureAwait(false);
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

        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        var tasks = taskFactories.Select(async factory =>
        {
            ArgumentNullException.ThrowIfNull(factory);
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                return await factory(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks).ConfigureAwait(false);
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
