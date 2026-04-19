using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.Infrastructure.Persistence.Models;

internal sealed class LinkClickRecord
{
    private LinkClickRecord()
    {
        IpAddress = string.Empty;
    }

    public Guid LinkClickId { get; private set; }

    public TenantId TenantId { get; private set; }

    public LinkId LinkId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? Referrer { get; private set; }
}