namespace Lynkly.Shared.Kernel.Observability.Metrics;

internal sealed class NoOpMetricsService : IMetricsService
{
    public void IncrementCounter(string name, long value = 1)
    {
    }
}
