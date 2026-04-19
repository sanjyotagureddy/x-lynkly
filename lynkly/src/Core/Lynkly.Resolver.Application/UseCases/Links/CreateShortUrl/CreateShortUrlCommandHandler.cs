using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core.Domain;
using Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed class CreateShortUrlCommandHandler(
    ILinkWriteRepository repository,
    IEncryptionService encryptionService,
    IMessagePublisher messagePublisher) : IRequestHandler<CreateShortUrlCommand, CreateShortUrlResult>
{
    private const int MaxAliasGenerationAttempts = 5;

    private readonly ILinkWriteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IEncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    private readonly IMessagePublisher _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));

    public async Task<CreateShortUrlResult> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var originalUrl = request.OriginalUrl.Trim();
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var destinationUri) ||
            (destinationUri.Scheme != Uri.UriSchemeHttp && destinationUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidDestinationUrlException(originalUrl);
        }

        var tenantId = await _repository.GetOrCreateDefaultTenantIdAsync(cancellationToken);
        var encryptedUrl = Convert.ToBase64String(_encryptionService.Encrypt(originalUrl, tenantId.ToString()));

        var link = Link.Create(tenantId, encryptedUrl, request.ExpiresAtUtc);
        var alias = await ResolveAliasAsync(tenantId, request.Alias, cancellationToken);
        var linkAlias = LinkAlias.Create(tenantId, link.Id, alias, isPrimary: true);

        _repository.Add(link, linkAlias);
        await _repository.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync(link.DomainEvents, cancellationToken);
        link.ClearDomainEvents();

        return new CreateShortUrlResult(link.Id.Value, linkAlias.Alias);
    }

    private async Task<string> ResolveAliasAsync(TenantId tenantId, string? requestedAlias, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedAlias))
        {
            var normalizedAlias = requestedAlias.Trim().ToLowerInvariant();
            var exists = await _repository.AliasExistsAsync(tenantId, normalizedAlias, cancellationToken);
            if (exists)
            {
                throw new AliasAlreadyExistsException(normalizedAlias);
            }

            return normalizedAlias;
        }

        for (var attempt = 0; attempt < MaxAliasGenerationAttempts; attempt++)
        {
            var generatedAlias = GenerateAlias();
            var exists = await _repository.AliasExistsAsync(tenantId, generatedAlias, cancellationToken);
            if (!exists)
            {
                return generatedAlias;
            }
        }

        throw new InvalidOperationException(
            $"Failed to generate a unique alias after {MaxAliasGenerationAttempts} attempts. Please retry or provide a custom alias.");
    }

    private static string GenerateAlias()
    {
        return Guid.NewGuid().ToString("N")[..8].ToLowerInvariant();
    }

    private Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        var publishTasks = domainEvents.Select(domainEvent => domainEvent switch
        {
            LinkCreatedDomainEvent linkCreatedDomainEvent =>
                _messagePublisher.PublishAsync(
                    new LinkCreatedMessage(
                        linkCreatedDomainEvent.LinkId.Value,
                        linkCreatedDomainEvent.TenantId.Value,
                        linkCreatedDomainEvent.DestinationUrl,
                        linkCreatedDomainEvent.OccurredOnUtc),
                    cancellationToken),
            _ => Task.CompletedTask
        });

        return Task.WhenAll(publishTasks);
    }
}
