using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.Domain.Links;

public sealed class LinkAlias : Entity<LinkAliasId>
{
    private LinkAlias()
        : base(default)
    {
        Alias = string.Empty;
        CreatedAtUtc = DateTimeOffset.UnixEpoch;
        UpdatedAtUtc = DateTimeOffset.UnixEpoch;
    }

    private LinkAlias(
        LinkAliasId id,
        TenantId tenantId,
        CustomDomainId? customDomainId,
        string alias,
        LinkId linkId,
        bool isPrimary,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        CustomDomainId = customDomainId;
        Alias = alias;
        LinkId = linkId;
        IsPrimary = isPrimary;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public TenantId TenantId { get; private set; }

    public CustomDomainId? CustomDomainId { get; private set; }

    public string Alias { get; private set; }

    public LinkId LinkId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static LinkAlias Create(
        TenantId tenantId,
        LinkId linkId,
        string alias,
        CustomDomainId? customDomainId = null,
        bool isPrimary = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        var now = DateTimeOffset.UtcNow;
        return new LinkAlias(
            LinkAliasId.New(),
            tenantId,
            customDomainId,
            alias.Trim().ToLowerInvariant(),
            linkId,
            isPrimary,
            now);
    }

    public static LinkAlias Rehydrate(
        LinkAliasId id,
        TenantId tenantId,
        CustomDomainId? customDomainId,
        string alias,
        LinkId linkId,
        bool isPrimary,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        return new LinkAlias
        {
            Id = id,
            TenantId = tenantId,
            CustomDomainId = customDomainId,
            Alias = alias.Trim().ToLowerInvariant(),
            LinkId = linkId,
            IsPrimary = isPrimary,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    public void SetPrimary(bool isPrimary)
    {
        if (IsPrimary == isPrimary)
        {
            return;
        }

        IsPrimary = isPrimary;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
