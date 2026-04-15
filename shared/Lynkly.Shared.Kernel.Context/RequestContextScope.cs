using System.Threading;

namespace Lynkly.Shared.Kernel.Context;

public static class RequestContextScope
{
    private static readonly AsyncLocal<AppContext?> CurrentContext = new();

    public static AppContext? Current => CurrentContext.Value;

    public static IDisposable BeginScope(AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(appContext);

        var previousContext = CurrentContext.Value;
        CurrentContext.Value = appContext;

        return new Scope(previousContext);
    }

    private sealed class Scope(AppContext? previousContext) : IDisposable
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
