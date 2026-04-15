using System.Diagnostics;

namespace Lynkly.Shared.Kernel.Observability.Tracing;

internal sealed class NoOpTracingService : ITracingService
{
    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return null;
    }
}
