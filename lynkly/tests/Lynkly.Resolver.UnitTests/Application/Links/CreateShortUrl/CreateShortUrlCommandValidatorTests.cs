using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

namespace Lynkly.Resolver.UnitTests.Application.Links.CreateShortUrl;

public sealed class CreateShortUrlCommandValidatorTests
{
    private readonly CreateShortUrlCommandValidator _validator = new();

    [Fact]
    public void Validate_ReturnsError_WhenOriginalUrlIsInvalid()
    {
        var command = new CreateShortUrlCommand("notaurl", null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateShortUrlCommand.OriginalUrl));
    }

    [Fact]
    public void Validate_ReturnsError_WhenAliasHasInvalidCharacters()
    {
        var command = new CreateShortUrlCommand("https://example.com/path", "bad alias", null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateShortUrlCommand.Alias));
    }

    [Fact]
    public void Validate_Succeeds_ForValidPayload()
    {
        var command = new CreateShortUrlCommand("https://example.com/path", "summer-sale", DateTimeOffset.UtcNow.AddDays(7));

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
