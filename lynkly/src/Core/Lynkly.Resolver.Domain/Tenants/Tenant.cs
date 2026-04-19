using Lynkly.Resolver.Domain.Links;
using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.Domain.Tenants;

public sealed class Tenant : AggregateRoot<TenantId>
{
    private Tenant()
        : base(default)
    {
        Name = string.Empty;
        Status = TenantStatus.Active;
        CreatedAtUtc = DateTimeOffset.UnixEpoch;
        UpdatedAtUtc = DateTimeOffset.UnixEpoch;
    }

    private Tenant(TenantId id, string name, DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = name;
        Status = TenantStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; }

    public TenantStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Tenant Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var now = DateTimeOffset.UtcNow;
        return new Tenant(TenantId.New(), name.Trim(), now);
    }

    public static Tenant Rehydrate(
        TenantId id,
        string name,
        TenantStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Tenant
        {
            Id = id,
            Name = name.Trim(),
            Status = status,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Touch();
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Suspended)
        {
            return;
        }

        Status = TenantStatus.Suspended;
        Touch();
    }

    public void Activate()
    {
        if (Status == TenantStatus.Active)
        {
            return;
        }

        Status = TenantStatus.Active;
        Touch();
    }

    public void Archive()
    {
        if (Status == TenantStatus.Archived)
        {
            return;
        }

        Status = TenantStatus.Archived;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
