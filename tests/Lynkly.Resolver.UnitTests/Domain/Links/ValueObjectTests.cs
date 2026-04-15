using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.UnitTests.Domain.Links;

public sealed class ValueObjectTests
{
    [Fact]
    public void LinkId_New_Should_GenerateUniqueIds()
    {
        var id1 = LinkId.New();
        var id2 = LinkId.New();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void LinkId_Should_StoreGuidValue()
    {
        var guid = Guid.NewGuid();
        var id = new LinkId(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void LinkId_ToString_Should_ReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new LinkId(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void LinkId_Equality_Should_WorkByValue()
    {
        var guid = Guid.NewGuid();
        var id1 = new LinkId(guid);
        var id2 = new LinkId(guid);

        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
    }

    [Fact]
    public void LinkId_Default_Should_HaveEmptyGuid()
    {
        var id = default(LinkId);

        Assert.Equal(Guid.Empty, id.Value);
    }

    [Fact]
    public void TenantId_New_Should_GenerateUniqueIds()
    {
        var id1 = TenantId.New();
        var id2 = TenantId.New();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TenantId_Should_StoreGuidValue()
    {
        var guid = Guid.NewGuid();
        var id = new TenantId(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void TenantId_ToString_Should_ReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new TenantId(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void TenantId_Equality_Should_WorkByValue()
    {
        var guid = Guid.NewGuid();
        var id1 = new TenantId(guid);
        var id2 = new TenantId(guid);

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void CustomDomainId_New_Should_GenerateUniqueIds()
    {
        var id1 = CustomDomainId.New();
        var id2 = CustomDomainId.New();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void CustomDomainId_Should_StoreGuidValue()
    {
        var guid = Guid.NewGuid();
        var id = new CustomDomainId(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void CustomDomainId_ToString_Should_ReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new CustomDomainId(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void CustomDomainId_Equality_Should_WorkByValue()
    {
        var guid = Guid.NewGuid();
        var id1 = new CustomDomainId(guid);
        var id2 = new CustomDomainId(guid);

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void LinkAliasId_New_Should_GenerateUniqueIds()
    {
        var id1 = LinkAliasId.New();
        var id2 = LinkAliasId.New();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void LinkAliasId_Should_StoreGuidValue()
    {
        var guid = Guid.NewGuid();
        var id = new LinkAliasId(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void LinkAliasId_ToString_Should_ReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new LinkAliasId(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void LinkAliasId_Equality_Should_WorkByValue()
    {
        var guid = Guid.NewGuid();
        var id1 = new LinkAliasId(guid);
        var id2 = new LinkAliasId(guid);

        Assert.Equal(id1, id2);
    }
}
