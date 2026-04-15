using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Domain;

public sealed class AggregateRootTests
{
    [Fact]
    public void DomainEvents_Should_BeEmptyByDefault()
    {
        var link = Link.Rehydrate(
            LinkId.New(), TenantId.New(), "https://example.com", LinkStatus.Active,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);

        Assert.Empty(link.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_Should_AddEvent_ViaCreateFactory()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        Assert.Single(link.DomainEvents);
        Assert.IsType<LinkCreatedDomainEvent>(link.DomainEvents.First());
    }

    [Fact]
    public void ClearDomainEvents_Should_RemoveAllEvents()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");
        Assert.NotEmpty(link.DomainEvents);

        link.ClearDomainEvents();

        Assert.Empty(link.DomainEvents);
    }

    [Fact]
    public void DomainEvents_Should_BeReadOnly()
    {
        var link = Link.Create(TenantId.New(), "https://example.com");

        var events = link.DomainEvents;

        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(events);
    }

    [Fact]
    public void ClearDomainEvents_OnEmptyCollection_Should_NotThrow()
    {
        var link = Link.Rehydrate(
            LinkId.New(), TenantId.New(), "https://example.com", LinkStatus.Active,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);

        var exception = Record.Exception(() => link.ClearDomainEvents());

        Assert.Null(exception);
    }
}
