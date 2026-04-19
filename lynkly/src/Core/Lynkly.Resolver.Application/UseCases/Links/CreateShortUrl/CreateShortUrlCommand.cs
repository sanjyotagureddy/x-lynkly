using Lynkly.Shared.Kernel.MediatR.Abstractions;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed record CreateShortUrlCommand(
    string OriginalUrl,
    string? Alias,
    DateTimeOffset? ExpiresAtUtc) : IRequest<CreateShortUrlResult>;
