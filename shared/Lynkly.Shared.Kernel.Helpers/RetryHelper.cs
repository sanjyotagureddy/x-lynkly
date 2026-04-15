namespace Lynkly.Shared.Kernel.Helpers;

/// <summary>
/// Defines supported retry delay strategies.
/// </summary>
public enum RetryDelayStrategy
{
    /// <summary>
    /// Uses a fixed delay between retries.
    /// </summary>
    Fixed,

    /// <summary>
    /// Uses exponential backoff delays.
    /// </summary>
    ExponentialBackoff
}

/// <summary>
/// Defines options for retry execution.
/// </summary>
public sealed class RetryPolicyOptions
{
    /// <summary>
    /// Maximum number of retries after the initial attempt.
    /// </summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>
    /// Initial delay before retries.
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Strategy used to calculate retry delays.
    /// </summary>
    public RetryDelayStrategy DelayStrategy { get; init; } = RetryDelayStrategy.ExponentialBackoff;

    /// <summary>
    /// Optional exception filter to decide retry eligibility.
    /// </summary>
    public Func<Exception, bool>? ExceptionFilter { get; init; }
}

/// <summary>
/// Provides async retry helper methods.
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Executes an async operation with retries.
    /// </summary>
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryPolicyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync<object?>(
            async ct =>
            {
                await operation(ct).ConfigureAwait(false);
                return null;
            },
            options,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async operation with retries and returns result.
    /// </summary>
    public static async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryPolicyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        options ??= new RetryPolicyOptions();

        if (options.RetryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.RetryCount), "Retry count cannot be negative.");
        }

        if (options.InitialDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options.InitialDelay), "Initial delay cannot be negative.");
        }

        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (ShouldRetry(exception, attempt, options))
            {
                var delay = CalculateDelay(attempt, options);
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private static bool ShouldRetry(Exception exception, int attempt, RetryPolicyOptions options)
    {
        var canRetry = attempt < options.RetryCount;
        var allowedByFilter = options.ExceptionFilter?.Invoke(exception) ?? true;
        return canRetry && allowedByFilter;
    }

    private static TimeSpan CalculateDelay(int attempt, RetryPolicyOptions options)
    {
        return options.DelayStrategy switch
        {
            RetryDelayStrategy.Fixed => options.InitialDelay,
            RetryDelayStrategy.ExponentialBackoff => ApplyJitter(
                TimeSpan.FromMilliseconds(options.InitialDelay.TotalMilliseconds * Math.Pow(2, attempt))),
            _ => options.InitialDelay
        };
    }

    private static TimeSpan ApplyJitter(TimeSpan delay)
    {
        if (delay <= TimeSpan.Zero)
        {
            return delay;
        }

        var jitterFactor = 0.8 + (Random.Shared.NextDouble() * 0.4);
        return TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitterFactor);
    }
}
