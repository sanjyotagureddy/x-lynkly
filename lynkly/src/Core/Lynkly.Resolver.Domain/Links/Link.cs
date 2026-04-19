using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.Domain.Links;

public sealed class Link : AggregateRoot<LinkId>
{
    private Link()
        : base(default)
    {
        DestinationUrl = string.Empty;
        Status = LinkStatus.Active;
        CreatedAtUtc = DateTimeOffset.UnixEpoch;
        UpdatedAtUtc = DateTimeOffset.UnixEpoch;
    }

    private Link(
        LinkId id,
        TenantId tenantId,
        string destinationUrl,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        DestinationUrl = destinationUrl;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        Status = LinkStatus.Active;
    }

    public TenantId TenantId { get; private set; }

    public string DestinationUrl { get; private set; }

    public LinkStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public static Link Create(TenantId tenantId, string destinationUrl, DateTimeOffset? expiresAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        var now = DateTimeOffset.UtcNow;
        var link = new Link(LinkId.New(), tenantId, destinationUrl.Trim(), now, expiresAtUtc);
        link.AddDomainEvent(new LinkCreatedDomainEvent(link.Id, tenantId, link.DestinationUrl, now.UtcDateTime));
        return link;
    }

    public static Link Rehydrate(
        LinkId id,
        TenantId tenantId,
        string destinationUrl,
        LinkStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        var link = new Link
        {
            Id = id,
            TenantId = tenantId,
            DestinationUrl = destinationUrl.Trim(),
            Status = status,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc,
            ExpiresAtUtc = expiresAtUtc
        };

        return link;
    }

    public void ChangeDestinationUrl(string destinationUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        DestinationUrl = destinationUrl.Trim();
        Touch();
    }

    public void Disable()
    {
        if (Status == LinkStatus.Disabled)
        {
            return;
        }

        Status = LinkStatus.Disabled;
        Touch();
    }

    public void Archive()
    {
        if (Status == LinkStatus.Archived)
        {
            return;
        }

        Status = LinkStatus.Archived;
        Touch();
    }

    public void Block()
    {
        if (Status == LinkStatus.Blocked)
        {
            return;
        }

        Status = LinkStatus.Blocked;
        Touch();
    }

    public void UpdateExpiration(DateTimeOffset? expiresAtUtc)
    {
        ExpiresAtUtc = expiresAtUtc;
        Touch();
    }

    public bool CanRedirect(DateTimeOffset utcNow)
    {
        if (Status != LinkStatus.Active)
        {
            return false;
        }

        return !ExpiresAtUtc.HasValue || ExpiresAtUtc > utcNow;
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
