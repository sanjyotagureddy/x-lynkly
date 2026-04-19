namespace Lynkly.Resolver.Application.BlockedDomains;

public sealed class BlockedDomainOptions
{
    public const string SectionName = "BlockedDomains";

    public IReadOnlyList<string> Domains { get; init; } = [];
}
