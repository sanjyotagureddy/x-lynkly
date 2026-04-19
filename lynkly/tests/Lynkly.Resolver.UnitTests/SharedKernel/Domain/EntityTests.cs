using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core.Domain;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Domain;

public sealed class EntityTests
{
    [Fact]
    public void Entities_WithSameId_Should_BeEqual()
    {
        var linkId = LinkId.New();
        var tenantId = TenantId.New();
        var now = DateTimeOffset.UtcNow;

        var link1 = Link.Rehydrate(linkId, tenantId, "https://a.com", LinkStatus.Active, now, now, null);
        var link2 = Link.Rehydrate(linkId, tenantId, "https://b.com", LinkStatus.Disabled, now, now, null);

        Assert.True(link1.Equals(link2));
        Assert.True(link1 == link2);
        Assert.False(link1 != link2);
    }

    [Fact]
    public void Entities_WithDifferentIds_Should_NotBeEqual()
    {
        var tenantId = TenantId.New();
        var now = DateTimeOffset.UtcNow;

        var link1 = Link.Rehydrate(LinkId.New(), tenantId, "https://a.com", LinkStatus.Active, now, now, null);
        var link2 = Link.Rehydrate(LinkId.New(), tenantId, "https://a.com", LinkStatus.Active, now, now, null);

        Assert.False(link1.Equals(link2));
        Assert.False(link1 == link2);
        Assert.True(link1 != link2);
    }

    [Fact]
    public void Entity_Equals_Null_Should_ReturnFalse()
    {
        var link = Link.Create(TenantId.New(), "https://a.com");

        Assert.False(link.Equals(null));
        Assert.False(link == null);
        Assert.True(link != null);
    }

    [Fact]
    public void Entity_Equals_Self_Should_ReturnTrue()
    {
        var link = Link.Create(TenantId.New(), "https://a.com");

        Assert.True(link.Equals(link));
#pragma warning disable CS1718 // Comparison to same variable - intentional equality operator test
        Assert.True(link == link);
#pragma warning restore CS1718
    }

    [Fact]
    public void Entity_Equals_Object_Should_ReturnTrue_WhenSameId()
    {
        var linkId = LinkId.New();
        var tenantId = TenantId.New();
        var now = DateTimeOffset.UtcNow;

        var link1 = Link.Rehydrate(linkId, tenantId, "https://a.com", LinkStatus.Active, now, now, null);
        var link2 = Link.Rehydrate(linkId, tenantId, "https://b.com", LinkStatus.Active, now, now, null);

        Assert.True(link1.Equals((object)link2));
    }

    [Fact]
    public void Entity_Equals_DifferentType_Should_ReturnFalse()
    {
        var link = Link.Create(TenantId.New(), "https://a.com");

        Assert.False(link.Equals("not an entity"));
        Assert.False(link.Equals(42));
    }

    [Fact]
    public void Entity_GetHashCode_Should_BeSame_ForEqualEntities()
    {
        var linkId = LinkId.New();
        var tenantId = TenantId.New();
        var now = DateTimeOffset.UtcNow;

        var link1 = Link.Rehydrate(linkId, tenantId, "https://a.com", LinkStatus.Active, now, now, null);
        var link2 = Link.Rehydrate(linkId, tenantId, "https://b.com", LinkStatus.Disabled, now, now, null);

        Assert.Equal(link1.GetHashCode(), link2.GetHashCode());
    }

    [Fact]
    public void Entity_GetHashCode_Should_BeDifferent_ForDifferentEntities()
    {
        var link1 = Link.Create(TenantId.New(), "https://a.com");
        var link2 = Link.Create(TenantId.New(), "https://b.com");

        Assert.NotEqual(link1.GetHashCode(), link2.GetHashCode());
    }

    [Fact]
    public void Null_Entities_Should_BeEqual()
    {
        Link? link1 = null;
        Link? link2 = null;

        Assert.True(link1 == link2);
        Assert.False(link1 != link2);
    }
}
