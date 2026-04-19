namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed record LinkCreatedMessage(
    Guid LinkId,
    Guid TenantId,
    string EncryptedDestinationUrl,
    DateTime OccurredOnUtc);
