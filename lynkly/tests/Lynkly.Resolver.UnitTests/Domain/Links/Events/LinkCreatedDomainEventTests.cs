using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.UnitTests.Domain.Links.Events;

public sealed class LinkCreatedDomainEventTests
{
    [Fact]
    public void Should_ImplementIDomainEvent()
    {
        var domainEvent = new LinkCreatedDomainEvent(
            LinkId.New(), TenantId.New(), "https://example.com", DateTime.UtcNow);

        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }

    [Fact]
    public void Should_StoreProperties()
    {
        var linkId = LinkId.New();
        var tenantId = TenantId.New();
        var url = "https://example.com";
        var occurredOn = DateTime.UtcNow;

        var domainEvent = new LinkCreatedDomainEvent(linkId, tenantId, url, occurredOn);

        Assert.Equal(linkId, domainEvent.LinkId);
        Assert.Equal(tenantId, domainEvent.TenantId);
        Assert.Equal(url, domainEvent.DestinationUrl);
        Assert.Equal(occurredOn, domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void Should_SupportRecordEquality()
    {
        var linkId = LinkId.New();
        var tenantId = TenantId.New();
        var now = DateTime.UtcNow;

        var event1 = new LinkCreatedDomainEvent(linkId, tenantId, "https://example.com", now);
        var event2 = new LinkCreatedDomainEvent(linkId, tenantId, "https://example.com", now);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void Should_NotBeEqual_WhenPropertiesDiffer()
    {
        var now = DateTime.UtcNow;

        var event1 = new LinkCreatedDomainEvent(LinkId.New(), TenantId.New(), "https://example.com", now);
        var event2 = new LinkCreatedDomainEvent(LinkId.New(), TenantId.New(), "https://other.com", now);

        Assert.NotEqual(event1, event2);
    }
}
