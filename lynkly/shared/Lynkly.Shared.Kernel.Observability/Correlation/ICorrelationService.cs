namespace Lynkly.Shared.Kernel.Observability.Correlation;

public interface ICorrelationService
{
    string GetOrCreateCorrelationId();
}
