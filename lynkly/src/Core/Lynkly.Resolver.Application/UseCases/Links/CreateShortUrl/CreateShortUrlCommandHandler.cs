using Lynkly.Resolver.Application.Abstractions;
using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core.Domain;
using Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;
using Lynkly.Shared.Kernel.Core.Helpers.Security;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed class CreateShortUrlCommandHandler(
    ILinkWriteRepository repository,
    IEncryptionService encryptionService,
    IShortAliasGenerator shortAliasGenerator,
    IMessagePublisher messagePublisher,
    IBlockedDomainChecker blockedDomainChecker,
    TimeProvider? timeProvider = null) : IRequestHandler<CreateShortUrlCommand, CreateShortUrlResult>
{
    private const int MaxAliasGenerationAttempts = 5;
    private static readonly TimeSpan DefaultLinkLifetime = TimeSpan.FromDays(30);

    private readonly ILinkWriteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IEncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    private readonly IShortAliasGenerator _shortAliasGenerator = shortAliasGenerator ?? throw new ArgumentNullException(nameof(shortAliasGenerator));
    private readonly IMessagePublisher _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    private readonly IBlockedDomainChecker _blockedDomainChecker = blockedDomainChecker ?? throw new ArgumentNullException(nameof(blockedDomainChecker));
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<CreateShortUrlResult> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var originalUrl = request.OriginalUrl.Trim();
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var destinationUri) ||
            (destinationUri.Scheme != Uri.UriSchemeHttp && destinationUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidDestinationUrlException(originalUrl);
        }

        if (_blockedDomainChecker.IsBlocked(destinationUri.Host))
        {
            throw new BlockedDomainException(destinationUri.Host);
        }

        var utcNow = _timeProvider.GetUtcNow();
        var expiresAtUtc = request.ExpiresAtUtc ?? utcNow.Add(DefaultLinkLifetime);

        var tenantId = await _repository.GetOrCreateDefaultTenantIdAsync(cancellationToken);
        var encryptedUrl = SecurityHelper.ToBase64(_encryptionService.Encrypt(originalUrl, tenantId.ToString()));

        var link = Link.Create(tenantId, encryptedUrl, expiresAtUtc);
        var alias = await ResolveAliasAsync(tenantId, originalUrl, request.Alias, cancellationToken);
        var linkAlias = LinkAlias.Create(tenantId, link.Id, alias, isPrimary: true);

        _repository.Add(link, linkAlias);
        await _repository.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync(link.DomainEvents, cancellationToken);
        link.ClearDomainEvents();

        return new CreateShortUrlResult(link.Id.Value, linkAlias.Alias);
    }

    private async Task<string> ResolveAliasAsync(TenantId tenantId, string originalUrl, string? requestedAlias, CancellationToken cancellationToken)
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
            var generatedAlias = _shortAliasGenerator.Generate(tenantId, originalUrl, attempt);
            var exists = await _repository.AliasExistsAsync(tenantId, generatedAlias, cancellationToken);
            if (!exists)
            {
                return generatedAlias;
            }
        }

        throw new InvalidOperationException(
            $"Failed to generate a unique alias after {MaxAliasGenerationAttempts} attempts. Please retry or provide a custom alias.");
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
