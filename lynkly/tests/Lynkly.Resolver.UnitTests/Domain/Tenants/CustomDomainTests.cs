using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;

namespace Lynkly.Resolver.UnitTests.Domain.Tenants;

public sealed class CustomDomainTests
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        var tenantId = TenantId.New();

        var domain = CustomDomain.Create(tenantId, "example.com");

        Assert.Equal(tenantId, domain.TenantId);
        Assert.Equal("example.com", domain.DomainName);
        Assert.Equal(DomainVerificationStatus.Pending, domain.VerificationStatus);
        Assert.NotEqual(default, domain.Id);
        Assert.True(domain.CreatedAtUtc <= DateTimeOffset.UtcNow);
        Assert.Equal(domain.CreatedAtUtc, domain.UpdatedAtUtc);
    }

    [Fact]
    public void Create_Should_NormalizeDomainName()
    {
        var domain = CustomDomain.Create(TenantId.New(), "  EXAMPLE.COM  ");

        Assert.Equal("example.com", domain.DomainName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Throw_WhenDomainNameIsNullOrWhiteSpace(string? domainName)
    {
        Assert.ThrowsAny<ArgumentException>(() => CustomDomain.Create(TenantId.New(), domainName!));
    }

    [Fact]
    public void MarkVerified_Should_UpdateStatus_AndTouchTimestamp()
    {
        var domain = CustomDomain.Create(TenantId.New(), "example.com");
        var initialUpdatedAt = domain.UpdatedAtUtc;

        domain.MarkVerified();

        Assert.Equal(DomainVerificationStatus.Verified, domain.VerificationStatus);
        Assert.True(domain.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void MarkFailed_Should_UpdateStatus_AndTouchTimestamp()
    {
        var domain = CustomDomain.Create(TenantId.New(), "example.com");
        var initialUpdatedAt = domain.UpdatedAtUtc;

        domain.MarkFailed();

        Assert.Equal(DomainVerificationStatus.Failed, domain.VerificationStatus);
        Assert.True(domain.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void Revoke_Should_UpdateStatus_AndTouchTimestamp()
    {
        var domain = CustomDomain.Create(TenantId.New(), "example.com");
        domain.MarkVerified();
        var initialUpdatedAt = domain.UpdatedAtUtc;

        domain.Revoke();

        Assert.Equal(DomainVerificationStatus.Revoked, domain.VerificationStatus);
        Assert.True(domain.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void VerificationFlow_Pending_To_Verified_To_Revoked()
    {
        var domain = CustomDomain.Create(TenantId.New(), "example.com");

        Assert.Equal(DomainVerificationStatus.Pending, domain.VerificationStatus);

        domain.MarkVerified();
        Assert.Equal(DomainVerificationStatus.Verified, domain.VerificationStatus);

        domain.Revoke();
        Assert.Equal(DomainVerificationStatus.Revoked, domain.VerificationStatus);
    }

    [Fact]
    public void VerificationFlow_Pending_To_Failed()
    {
        var domain = CustomDomain.Create(TenantId.New(), "example.com");

        domain.MarkFailed();

        Assert.Equal(DomainVerificationStatus.Failed, domain.VerificationStatus);
    }

    [Fact]
    public void Rehydrate_Should_RestoreExistingState()
    {
        var customDomainId = CustomDomainId.New();
        var tenantId = TenantId.New();
        var createdAtUtc = DateTimeOffset.Parse("2026-01-01T00:00:00+00:00");
        var updatedAtUtc = DateTimeOffset.Parse("2026-01-02T00:00:00+00:00");

        var domain = CustomDomain.Rehydrate(
            customDomainId, tenantId, "example.com", DomainVerificationStatus.Verified, createdAtUtc, updatedAtUtc);

        Assert.Equal(customDomainId, domain.Id);
        Assert.Equal(tenantId, domain.TenantId);
        Assert.Equal("example.com", domain.DomainName);
        Assert.Equal(DomainVerificationStatus.Verified, domain.VerificationStatus);
        Assert.Equal(createdAtUtc, domain.CreatedAtUtc);
        Assert.Equal(updatedAtUtc, domain.UpdatedAtUtc);
    }

    [Fact]
    public void Rehydrate_Should_NormalizeDomainName()
    {
        var now = DateTimeOffset.UtcNow;

        var domain = CustomDomain.Rehydrate(
            CustomDomainId.New(), TenantId.New(), "  EXAMPLE.COM  ",
            DomainVerificationStatus.Pending, now, now);

        Assert.Equal("example.com", domain.DomainName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rehydrate_Should_Throw_WhenDomainNameIsNullOrWhiteSpace(string? domainName)
    {
        var now = DateTimeOffset.UtcNow;

        Assert.ThrowsAny<ArgumentException>(() =>
            CustomDomain.Rehydrate(CustomDomainId.New(), TenantId.New(), domainName!,
                DomainVerificationStatus.Pending, now, now));
    }
}
