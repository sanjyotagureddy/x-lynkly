using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;

namespace Lynkly.Resolver.UnitTests.Domain.Links;

public sealed class LinkTests
{
    [Fact]
    public void Create_Should_SetProperties_AndRaiseCreatedEvent()
    {
        var tenantId = TenantId.New();

        var link = Link.Create(tenantId, "https://example.com/home", DateTimeOffset.Parse("2026-12-31T23:59:59+00:00"));

        Assert.Equal(tenantId, link.TenantId);
        Assert.Equal("https://example.com/home", link.DestinationUrl);
        Assert.Equal(LinkStatus.Active, link.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-12-31T23:59:59+00:00"), link.ExpiresAtUtc);
        Assert.True(link.CreatedAtUtc <= link.UpdatedAtUtc);

        var domainEvent = Assert.Single(link.DomainEvents);
        var createdEvent = Assert.IsType<LinkCreatedDomainEvent>(domainEvent);
        Assert.Equal(link.Id, createdEvent.LinkId);
        Assert.Equal(tenantId, createdEvent.TenantId);
        Assert.Equal(link.DestinationUrl, createdEvent.DestinationUrl);
    }

    [Fact]
    public void ChangeDestinationUrl_Should_UpdateDestination_AndTouchTimestamp()
    {
        var link = Link.Create(TenantId.New(), "https://example.com/home");
        var updatedAtUtc = link.UpdatedAtUtc;

        link.ChangeDestinationUrl("https://example.com/new-home");

        Assert.Equal("https://example.com/new-home", link.DestinationUrl);
        Assert.True(link.UpdatedAtUtc >= updatedAtUtc);
    }

    [Fact]
    public void Disable_And_Archive_Should_UpdateStatus()
    {
        var link = Link.Create(TenantId.New(), "https://example.com/home");

        link.Disable();

        Assert.Equal(LinkStatus.Disabled, link.Status);

        link.Archive();

        Assert.Equal(LinkStatus.Archived, link.Status);
    }

    [Fact]
    public void Block_Should_UpdateStatus()
    {
        var link = Link.Create(TenantId.New(), "https://example.com/home");

        link.Block();

        Assert.Equal(LinkStatus.Blocked, link.Status);
    }

    [Fact]
    public void CanRedirect_Should_ReturnFalse_WhenBlockedOrExpired()
    {
        var tenantId = TenantId.New();
        var blockedLink = Link.Create(tenantId, "https://example.com/home");
        blockedLink.Block();

        var expiredLink = Link.Create(tenantId, "https://example.com/home", DateTimeOffset.Parse("2026-04-01T00:00:00+00:00"));

        Assert.False(blockedLink.CanRedirect(DateTimeOffset.UtcNow));
        Assert.False(expiredLink.CanRedirect(DateTimeOffset.Parse("2026-04-15T00:00:00+00:00")));
    }

    [Fact]
    public void ClearDomainEvents_Should_RemoveAllEvents()
    {
        var link = Link.Create(TenantId.New(), "https://example.com/home");

        link.ClearDomainEvents();

        Assert.Empty(link.DomainEvents);
    }

    [Fact]
    public void Rehydrate_Should_RestoreExistingState()
    {
        var linkId = LinkId.New();
        var tenantId = TenantId.New();
        var createdAtUtc = DateTimeOffset.Parse("2026-04-01T10:00:00+00:00");
        var updatedAtUtc = DateTimeOffset.Parse("2026-04-02T10:00:00+00:00");

        var link = Link.Rehydrate(
            linkId,
            tenantId,
            "https://example.com/home",
            LinkStatus.Disabled,
            createdAtUtc,
            updatedAtUtc,
            null);

        Assert.Equal(linkId, link.Id);
        Assert.Equal(tenantId, link.TenantId);
        Assert.Equal(LinkStatus.Disabled, link.Status);
        Assert.Equal(createdAtUtc, link.CreatedAtUtc);
        Assert.Equal(updatedAtUtc, link.UpdatedAtUtc);
        Assert.Empty(link.DomainEvents);
    }
}
