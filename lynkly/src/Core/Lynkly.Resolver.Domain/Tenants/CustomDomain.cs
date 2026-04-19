using Lynkly.Resolver.Domain.Links;
using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.Domain.Tenants;

public sealed class CustomDomain : Entity<CustomDomainId>
{
    private CustomDomain()
        : base(default)
    {
        DomainName = string.Empty;
        VerificationStatus = DomainVerificationStatus.Pending;
        CreatedAtUtc = DateTimeOffset.UnixEpoch;
        UpdatedAtUtc = DateTimeOffset.UnixEpoch;
    }

    private CustomDomain(CustomDomainId id, TenantId tenantId, string domainName, DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        DomainName = domainName;
        VerificationStatus = DomainVerificationStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public TenantId TenantId { get; private set; }

    public string DomainName { get; private set; }

    public DomainVerificationStatus VerificationStatus { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static CustomDomain Create(TenantId tenantId, string domainName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainName);

        var now = DateTimeOffset.UtcNow;
        return new CustomDomain(CustomDomainId.New(), tenantId, domainName.Trim().ToLowerInvariant(), now);
    }

    public static CustomDomain Rehydrate(
        CustomDomainId id,
        TenantId tenantId,
        string domainName,
        DomainVerificationStatus verificationStatus,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainName);

        return new CustomDomain
        {
            Id = id,
            TenantId = tenantId,
            DomainName = domainName.Trim().ToLowerInvariant(),
            VerificationStatus = verificationStatus,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    public void MarkVerified()
    {
        VerificationStatus = DomainVerificationStatus.Verified;
        Touch();
    }

    public void MarkFailed()
    {
        VerificationStatus = DomainVerificationStatus.Failed;
        Touch();
    }

    public void Revoke()
    {
        VerificationStatus = DomainVerificationStatus.Revoked;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
