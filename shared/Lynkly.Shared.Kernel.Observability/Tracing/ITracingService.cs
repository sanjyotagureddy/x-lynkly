using System.Diagnostics;

namespace Lynkly.Shared.Kernel.Observability.Tracing;

public interface ITracingService
{
    Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal);
}
