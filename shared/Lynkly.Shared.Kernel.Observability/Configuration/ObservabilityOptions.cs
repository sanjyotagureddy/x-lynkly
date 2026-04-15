namespace Lynkly.Shared.Kernel.Observability.Configuration;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string? ServiceName { get; init; }

    public string? ServiceVersion { get; init; }
}
