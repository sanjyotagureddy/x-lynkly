using Lynkly.Resolver.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Lynkly.Resolver.Application.BlockedDomains;

internal sealed class ConfigurableBlockedDomainChecker(IOptions<BlockedDomainOptions> options) : IBlockedDomainChecker
{
    private readonly HashSet<string> _blocked = new(
        options.Value.Domains.Select(d => d.Trim().ToLowerInvariant()),
        StringComparer.OrdinalIgnoreCase);

    public bool IsBlocked(string host)
    {
        ArgumentNullException.ThrowIfNull(host);
        return _blocked.Contains(host.Trim().ToLowerInvariant());
    }
}
