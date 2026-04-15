namespace Lynkly.Shared.Kernel.Observability.Correlation;

internal sealed class NoOpCorrelationService : ICorrelationService
{
    private const string PlaceholderCorrelationId = "correlation-id-placeholder";

    public string GetOrCreateCorrelationId()
    {
        return PlaceholderCorrelationId;
    }
}
