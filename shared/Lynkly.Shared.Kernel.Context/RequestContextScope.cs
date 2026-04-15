namespace Lynkly.Shared.Kernel.Context;

public static class RequestContextScope
{
    private static readonly AsyncLocal<AppContext?> CurrentContext = new();

    public static AppContext? Current => CurrentContext.Value;

    public static IDisposable BeginScope(AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(appContext);

        var previous = CurrentContext.Value;
        CurrentContext.Value = appContext;

        return new Scope(() => CurrentContext.Value = previous);
    }

    private sealed class Scope(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _onDispose();
        }
    }
}
