namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed class AliasGeneratorOptions
{
    public const string SectionName = "AliasGenerator";

    /// <summary>
    /// Secret pepper used as the HMAC key when generating short aliases.
    /// Must be set to a non-empty secret value in production configuration.
    /// </summary>
    public string HmacKey { get; init; } = string.Empty;
}
