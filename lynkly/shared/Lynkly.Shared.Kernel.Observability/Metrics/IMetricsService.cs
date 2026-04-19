namespace Lynkly.Shared.Kernel.Observability.Metrics;

public interface IMetricsService
{
    void IncrementCounter(string name, long value = 1);
}
