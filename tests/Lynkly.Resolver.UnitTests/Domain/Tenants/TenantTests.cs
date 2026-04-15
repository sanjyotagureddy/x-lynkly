using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Tenants;

namespace Lynkly.Resolver.UnitTests.Domain.Tenants;

public sealed class TenantTests
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        var tenant = Tenant.Create("Acme Corp");

        Assert.Equal("Acme Corp", tenant.Name);
        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.NotEqual(default, tenant.Id);
        Assert.True(tenant.CreatedAtUtc <= DateTimeOffset.UtcNow);
        Assert.Equal(tenant.CreatedAtUtc, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void Create_Should_TrimName()
    {
        var tenant = Tenant.Create("  Acme Corp  ");

        Assert.Equal("Acme Corp", tenant.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Throw_WhenNameIsNullOrWhiteSpace(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => Tenant.Create(name!));
    }

    [Fact]
    public void Rename_Should_UpdateName_AndTouchTimestamp()
    {
        var tenant = Tenant.Create("Acme Corp");
        var initialUpdatedAt = tenant.UpdatedAtUtc;

        tenant.Rename("New Acme");

        Assert.Equal("New Acme", tenant.Name);
        Assert.True(tenant.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void Rename_Should_TrimName()
    {
        var tenant = Tenant.Create("Acme Corp");

        tenant.Rename("  Trimmed Name  ");

        Assert.Equal("Trimmed Name", tenant.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_Should_Throw_WhenNameIsNullOrWhiteSpace(string? name)
    {
        var tenant = Tenant.Create("Acme Corp");

        Assert.ThrowsAny<ArgumentException>(() => tenant.Rename(name!));
    }

    [Fact]
    public void Suspend_Should_SetStatusToSuspended()
    {
        var tenant = Tenant.Create("Acme Corp");
        var initialUpdatedAt = tenant.UpdatedAtUtc;

        tenant.Suspend();

        Assert.Equal(TenantStatus.Suspended, tenant.Status);
        Assert.True(tenant.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void Suspend_Should_BeIdempotent()
    {
        var tenant = Tenant.Create("Acme Corp");
        tenant.Suspend();
        var updatedAfterFirstSuspend = tenant.UpdatedAtUtc;

        tenant.Suspend();

        Assert.Equal(TenantStatus.Suspended, tenant.Status);
        Assert.Equal(updatedAfterFirstSuspend, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void Activate_Should_SetStatusToActive()
    {
        var tenant = Tenant.Create("Acme Corp");
        tenant.Suspend();
        var initialUpdatedAt = tenant.UpdatedAtUtc;

        tenant.Activate();

        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.True(tenant.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void Activate_Should_BeIdempotent_WhenAlreadyActive()
    {
        var tenant = Tenant.Create("Acme Corp");
        var initialUpdatedAt = tenant.UpdatedAtUtc;

        tenant.Activate();

        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.Equal(initialUpdatedAt, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void Archive_Should_SetStatusToArchived()
    {
        var tenant = Tenant.Create("Acme Corp");
        var initialUpdatedAt = tenant.UpdatedAtUtc;

        tenant.Archive();

        Assert.Equal(TenantStatus.Archived, tenant.Status);
        Assert.True(tenant.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void Archive_Should_BeIdempotent()
    {
        var tenant = Tenant.Create("Acme Corp");
        tenant.Archive();
        var updatedAfterFirstArchive = tenant.UpdatedAtUtc;

        tenant.Archive();

        Assert.Equal(TenantStatus.Archived, tenant.Status);
        Assert.Equal(updatedAfterFirstArchive, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void Rehydrate_Should_RestoreExistingState()
    {
        var tenantId = TenantId.New();
        var createdAtUtc = DateTimeOffset.Parse("2026-01-01T00:00:00+00:00");
        var updatedAtUtc = DateTimeOffset.Parse("2026-01-02T00:00:00+00:00");

        var tenant = Tenant.Rehydrate(tenantId, "Acme Corp", TenantStatus.Suspended, createdAtUtc, updatedAtUtc);

        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal("Acme Corp", tenant.Name);
        Assert.Equal(TenantStatus.Suspended, tenant.Status);
        Assert.Equal(createdAtUtc, tenant.CreatedAtUtc);
        Assert.Equal(updatedAtUtc, tenant.UpdatedAtUtc);
    }

    [Fact]
    public void Rehydrate_Should_TrimName()
    {
        var tenantId = TenantId.New();
        var now = DateTimeOffset.UtcNow;

        var tenant = Tenant.Rehydrate(tenantId, "  Acme  ", TenantStatus.Active, now, now);

        Assert.Equal("Acme", tenant.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rehydrate_Should_Throw_WhenNameIsNullOrWhiteSpace(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Tenant.Rehydrate(TenantId.New(), name!, TenantStatus.Active, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void StatusTransition_Suspend_Then_Activate_Then_Archive()
    {
        var tenant = Tenant.Create("Acme Corp");

        tenant.Suspend();
        Assert.Equal(TenantStatus.Suspended, tenant.Status);

        tenant.Activate();
        Assert.Equal(TenantStatus.Active, tenant.Status);

        tenant.Archive();
        Assert.Equal(TenantStatus.Archived, tenant.Status);
    }

    [Fact]
    public void Rehydrate_Should_NotRaiseDomainEvents()
    {
        var tenant = Tenant.Rehydrate(
            TenantId.New(), "Acme", TenantStatus.Active, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        Assert.Empty(tenant.DomainEvents);
    }
}
