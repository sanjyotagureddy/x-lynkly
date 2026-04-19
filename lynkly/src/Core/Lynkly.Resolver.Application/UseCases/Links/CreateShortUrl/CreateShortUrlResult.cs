namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed record CreateShortUrlResult(Guid LinkId, string Alias);
