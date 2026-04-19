using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.UnitTests.Application.Links.CreateShortUrl;

public sealed class Sha256ShortAliasGeneratorTests
{
    private static readonly TenantId TenantId = new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

    [Fact]
    public void Generate_Should_BeDeterministic_ForSameInputs()
    {
        var generator = new Sha256ShortAliasGenerator();

        var first = generator.Generate(TenantId, "https://example.com/path", 0);
        var second = generator.Generate(TenantId, "https://example.com/path", 0);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Generate_Should_Return_EightCharAlias()
    {
        var generator = new Sha256ShortAliasGenerator();

        var alias = generator.Generate(TenantId, "https://example.com/path", 0);

        Assert.Equal(8, alias.Length);
    }

    [Fact]
    public void Generate_Should_Change_WhenAttemptChanges()
    {
        var generator = new Sha256ShortAliasGenerator();

        var first = generator.Generate(TenantId, "https://example.com/path", 0);
        var second = generator.Generate(TenantId, "https://example.com/path", 1);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Generate_Should_Change_WhenTenantChanges()
    {
        var generator = new Sha256ShortAliasGenerator();

        var first = generator.Generate(TenantId, "https://example.com/path", 0);
        var second = generator.Generate(new TenantId(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")), "https://example.com/path", 0);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Generate_Should_Throw_WhenOriginalUrlIsNull()
    {
        var generator = new Sha256ShortAliasGenerator();

        Assert.Throws<ArgumentNullException>(() => generator.Generate(TenantId, null!, 0));
    }

    [Fact]
    public void Generate_Should_Throw_WhenAttemptIsNegative()
    {
        var generator = new Sha256ShortAliasGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(TenantId, "https://example.com/path", -1));
    }
}
