namespace Lynkly.Shared.Kernel.Security.Configuration;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public string? Authority { get; init; }

    public string? Audience { get; init; }
}
