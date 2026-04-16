namespace Lynkly.Shared.Kernel.Context;

/// <summary>
/// Provides an ambient, <see cref="System.Threading.AsyncLocal{T}"/>-based scope
/// for propagating an <see cref="AppCallContext"/> through asynchronous call stacks
/// without explicit parameter passing.
/// </summary>
public static class RequestContextScope
{
    private static readonly AsyncLocal<AppCallContext?> CurrentContext = new();

    /// <summary>
    /// Gets the <see cref="AppCallContext"/> ambient for the current asynchronous execution context,
    /// or <see langword="null"/> if no scope has been established.
    /// </summary>
    public static AppCallContext? Current => CurrentContext.Value;

    /// <summary>
    /// Establishes <paramref name="appCallContext"/> as the ambient context for the current
    /// asynchronous execution context.
    /// </summary>
    /// <param name="appCallContext">The context to make ambient. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// A disposable that, when disposed, restores the context that was ambient before this call.
    /// Double-disposing is safe and has no effect beyond the first call.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="appCallContext"/> is <see langword="null"/>.
    /// </exception>
    public static IDisposable BeginScope(AppCallContext appCallContext)
    {
        ArgumentNullException.ThrowIfNull(appCallContext);

        var previousContext = CurrentContext.Value;
        CurrentContext.Value = appCallContext;

        return new Scope(previousContext);
    }

    private sealed class Scope(AppCallContext? previousContext) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CurrentContext.Value = previousContext;
            _disposed = true;
        }
    }
}
