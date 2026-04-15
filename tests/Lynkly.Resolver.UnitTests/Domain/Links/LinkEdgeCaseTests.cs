using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;

namespace Lynkly.Resolver.UnitTests.Domain.Links;

public sealed class LinkEdgeCaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Throw_WhenDestinationUrlIsNullOrWhiteSpace(string? url)
    {
        Assert.ThrowsAny<ArgumentException>(() => Link.Create(TenantId.New(), url!));
    }

    [Fact]
    public void Create_Should_TrimDestinationUrl()
    {
        var link = Link.Create(TenantId.New(), "  https://example.com  ");

        Assert.Equal("https://example.com", link.DestinationUrl);
    }

    [Fact]
    public void Create_WithoutExpiration_Should_LeaveExpiresAtUtcNull()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        Assert.Null(link.ExpiresAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeDestinationUrl_Should_Throw_WhenUrlIsNullOrWhiteSpace(string? url)
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        Assert.ThrowsAny<ArgumentException>(() => link.ChangeDestinationUrl(url!));
    }

    [Fact]
    public void ChangeDestinationUrl_Should_TrimUrl()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        link.ChangeDestinationUrl("  https://other.com  ");

        Assert.Equal("https://other.com", link.DestinationUrl);
    }

    [Fact]
    public void Disable_Should_BeIdempotent()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        link.Disable();
        var updatedAfterFirst = link.UpdatedAtUtc;

        link.Disable();

        Assert.Equal(LinkStatus.Disabled, link.Status);
        Assert.Equal(updatedAfterFirst, link.UpdatedAtUtc);
    }

    [Fact]
    public void Archive_Should_BeIdempotent()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        link.Archive();
        var updatedAfterFirst = link.UpdatedAtUtc;

        link.Archive();

        Assert.Equal(LinkStatus.Archived, link.Status);
        Assert.Equal(updatedAfterFirst, link.UpdatedAtUtc);
    }

    [Fact]
    public void Block_Should_BeIdempotent()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        link.Block();
        var updatedAfterFirst = link.UpdatedAtUtc;

        link.Block();

        Assert.Equal(LinkStatus.Blocked, link.Status);
        Assert.Equal(updatedAfterFirst, link.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateExpiration_Should_SetNewExpiration()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        var newExpiry = DateTimeOffset.Parse("2027-12-31T23:59:59+00:00");

        link.UpdateExpiration(newExpiry);

        Assert.Equal(newExpiry, link.ExpiresAtUtc);
    }

    [Fact]
    public void UpdateExpiration_Should_ClearExpiration_WhenSetToNull()
    {
        var link = Link.Create(TenantId.New(), "https://example.com",
            DateTimeOffset.Parse("2027-12-31T23:59:59+00:00"));

        link.UpdateExpiration(null);

        Assert.Null(link.ExpiresAtUtc);
    }

    [Fact]
    public void UpdateExpiration_Should_TouchTimestamp()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        var initialUpdatedAt = link.UpdatedAtUtc;

        link.UpdateExpiration(DateTimeOffset.Parse("2027-12-31T23:59:59+00:00"));

        Assert.True(link.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void CanRedirect_Should_ReturnTrue_WhenActiveAndNotExpired()
    {
        var link = Link.Create(TenantId.New(), "https://example.com",
            DateTimeOffset.Parse("2027-12-31T23:59:59+00:00"));

        Assert.True(link.CanRedirect(DateTimeOffset.Parse("2026-06-15T00:00:00+00:00")));
    }

    [Fact]
    public void CanRedirect_Should_ReturnTrue_WhenActiveWithNoExpiration()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        Assert.True(link.CanRedirect(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CanRedirect_Should_ReturnFalse_WhenDisabled()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        link.Disable();

        Assert.False(link.CanRedirect(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CanRedirect_Should_ReturnFalse_WhenArchived()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        link.Archive();

        Assert.False(link.CanRedirect(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CanRedirect_Should_ReturnFalse_WhenExpired()
    {
        var link = Link.Create(TenantId.New(), "https://example.com",
            DateTimeOffset.Parse("2025-01-01T00:00:00+00:00"));

        Assert.False(link.CanRedirect(DateTimeOffset.Parse("2026-01-01T00:00:00+00:00")));
    }

    [Fact]
    public void CanRedirect_Should_ReturnFalse_WhenExpirationExactlyEqualsNow()
    {
        var exactTime = DateTimeOffset.Parse("2026-06-15T12:00:00+00:00");
        var link = Link.Create(TenantId.New(), "https://example.com", exactTime);

        Assert.False(link.CanRedirect(exactTime));
    }

    [Fact]
    public void Create_DomainEvent_Should_HaveCorrectProperties()
    {
        var tenantId = TenantId.New();
        var link = Link.Create(tenantId, "https://example.com");

        var domainEvent = Assert.Single(link.DomainEvents);
        var createdEvent = Assert.IsType<LinkCreatedDomainEvent>(domainEvent);

        Assert.Equal(link.Id, createdEvent.LinkId);
        Assert.Equal(tenantId, createdEvent.TenantId);
        Assert.Equal("https://example.com", createdEvent.DestinationUrl);
        Assert.True(createdEvent.OccurredOnUtc <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rehydrate_Should_Throw_WhenDestinationUrlIsNullOrWhiteSpace(string? url)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Link.Rehydrate(LinkId.New(), TenantId.New(), url!, LinkStatus.Active,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null));
    }

    [Fact]
    public void Rehydrate_Should_TrimDestinationUrl()
    {
        var link = Link.Rehydrate(
            LinkId.New(), TenantId.New(), "  https://example.com  ", LinkStatus.Active,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);

        Assert.Equal("https://example.com", link.DestinationUrl);
    }

    [Fact]
    public void Rehydrate_Should_NotRaiseDomainEvents()
    {
        var link = Link.Rehydrate(
            LinkId.New(), TenantId.New(), "https://example.com", LinkStatus.Active,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);

        Assert.Empty(link.DomainEvents);
    }

    [Fact]
    public void Rehydrate_Should_RestoreExpiration()
    {
        var expiry = DateTimeOffset.Parse("2027-12-31T23:59:59+00:00");

        var link = Link.Rehydrate(
            LinkId.New(), TenantId.New(), "https://example.com", LinkStatus.Active,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, expiry);

        Assert.Equal(expiry, link.ExpiresAtUtc);
    }

    [Fact]
    public void StatusTransition_Active_Disable_Block()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        Assert.Equal(LinkStatus.Active, link.Status);

        link.Disable();
        Assert.Equal(LinkStatus.Disabled, link.Status);

        link.Block();
        Assert.Equal(LinkStatus.Blocked, link.Status);
    }
}
