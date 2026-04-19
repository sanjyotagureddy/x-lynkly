using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.UnitTests.Domain.Links;

public sealed class LinkAliasTests
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        var tenantId = TenantId.New();
        var linkId = LinkId.New();

        var alias = LinkAlias.Create(tenantId, linkId, "my-slug");

        Assert.Equal(tenantId, alias.TenantId);
        Assert.Equal(linkId, alias.LinkId);
        Assert.Equal("my-slug", alias.Alias);
        Assert.False(alias.IsPrimary);
        Assert.Null(alias.CustomDomainId);
        Assert.NotEqual(default, alias.Id);
        Assert.True(alias.CreatedAtUtc <= DateTimeOffset.UtcNow);
        Assert.Equal(alias.CreatedAtUtc, alias.UpdatedAtUtc);
    }

    [Fact]
    public void Create_Should_NormalizeAlias()
    {
        var alias = LinkAlias.Create(TenantId.New(), LinkId.New(), "  MY-SLUG  ");

        Assert.Equal("my-slug", alias.Alias);
    }

    [Fact]
    public void Create_Should_SetIsPrimary_WhenSpecified()
    {
        var alias = LinkAlias.Create(TenantId.New(), LinkId.New(), "my-slug", isPrimary: true);

        Assert.True(alias.IsPrimary);
    }

    [Fact]
    public void Create_Should_SetCustomDomainId_WhenSpecified()
    {
        var customDomainId = CustomDomainId.New();

        var alias = LinkAlias.Create(TenantId.New(), LinkId.New(), "my-slug", customDomainId);

        Assert.Equal(customDomainId, alias.CustomDomainId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Throw_WhenAliasIsNullOrWhiteSpace(string? aliasValue)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            LinkAlias.Create(TenantId.New(), LinkId.New(), aliasValue!));
    }

    [Fact]
    public void SetPrimary_Should_UpdateIsPrimary_AndTouchTimestamp()
    {
        var alias = LinkAlias.Create(TenantId.New(), LinkId.New(), "my-slug");
        var initialUpdatedAt = alias.UpdatedAtUtc;

        alias.SetPrimary(true);

        Assert.True(alias.IsPrimary);
        Assert.True(alias.UpdatedAtUtc >= initialUpdatedAt);
    }

    [Fact]
    public void SetPrimary_Should_BeIdempotent_WhenValueUnchanged()
    {
        var alias = LinkAlias.Create(TenantId.New(), LinkId.New(), "my-slug", isPrimary: true);
        var initialUpdatedAt = alias.UpdatedAtUtc;

        alias.SetPrimary(true);

        Assert.True(alias.IsPrimary);
        Assert.Equal(initialUpdatedAt, alias.UpdatedAtUtc);
    }

    [Fact]
    public void SetPrimary_Should_UnsetPrimary()
    {
        var alias = LinkAlias.Create(TenantId.New(), LinkId.New(), "my-slug", isPrimary: true);

        alias.SetPrimary(false);

        Assert.False(alias.IsPrimary);
    }

    [Fact]
    public void Rehydrate_Should_RestoreExistingState()
    {
        var aliasId = LinkAliasId.New();
        var tenantId = TenantId.New();
        var customDomainId = CustomDomainId.New();
        var linkId = LinkId.New();
        var createdAtUtc = DateTimeOffset.Parse("2026-01-01T00:00:00+00:00");
        var updatedAtUtc = DateTimeOffset.Parse("2026-01-02T00:00:00+00:00");

        var alias = LinkAlias.Rehydrate(
            aliasId, tenantId, customDomainId, "my-slug", linkId, true, createdAtUtc, updatedAtUtc);

        Assert.Equal(aliasId, alias.Id);
        Assert.Equal(tenantId, alias.TenantId);
        Assert.Equal(customDomainId, alias.CustomDomainId);
        Assert.Equal("my-slug", alias.Alias);
        Assert.Equal(linkId, alias.LinkId);
        Assert.True(alias.IsPrimary);
        Assert.Equal(createdAtUtc, alias.CreatedAtUtc);
        Assert.Equal(updatedAtUtc, alias.UpdatedAtUtc);
    }

    [Fact]
    public void Rehydrate_Should_NormalizeAlias()
    {
        var now = DateTimeOffset.UtcNow;

        var alias = LinkAlias.Rehydrate(
            LinkAliasId.New(), TenantId.New(), null, "  MY-SLUG  ",
            LinkId.New(), false, now, now);

        Assert.Equal("my-slug", alias.Alias);
    }

    [Fact]
    public void Rehydrate_Should_AllowNullCustomDomainId()
    {
        var now = DateTimeOffset.UtcNow;

        var alias = LinkAlias.Rehydrate(
            LinkAliasId.New(), TenantId.New(), null, "my-slug",
            LinkId.New(), false, now, now);

        Assert.Null(alias.CustomDomainId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rehydrate_Should_Throw_WhenAliasIsNullOrWhiteSpace(string? aliasValue)
    {
        var now = DateTimeOffset.UtcNow;

        Assert.ThrowsAny<ArgumentException>(() =>
            LinkAlias.Rehydrate(LinkAliasId.New(), TenantId.New(), null, aliasValue!,
                LinkId.New(), false, now, now));
    }
}
