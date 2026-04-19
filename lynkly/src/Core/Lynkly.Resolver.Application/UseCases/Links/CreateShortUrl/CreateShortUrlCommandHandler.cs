using Lynkly.Resolver.Application.Abstractions;
using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Application.UseCases.Links;
using Lynkly.Resolver.Domain.Links;
using Lynkly.Resolver.Domain.Links.Events;
using Lynkly.Shared.Kernel.Core;
using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Core.Domain;
using Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;
using Lynkly.Shared.Kernel.Core.Helpers.Security;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;
using ResolverAppContext = Lynkly.Shared.Kernel.Core.AppContext;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public sealed class CreateShortUrlCommandHandler(
    ILinkWriteRepository repository,
    IEncryptionService encryptionService,
    IShortAliasGenerator shortAliasGenerator,
    IMessagePublisher messagePublisher,
    ICacheService cacheService,
    IBlockedDomainChecker blockedDomainChecker,
    IStructuredLogger<CreateShortUrlCommandHandler> logger,
    TimeProvider? timeProvider = null) : IRequestHandler<CreateShortUrlCommand, CreateShortUrlResult>
{
    private const int MaxAliasGenerationAttempts = 5;
    private static readonly TimeSpan DefaultLinkLifetime = TimeSpan.FromDays(30);

    private readonly ILinkWriteRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IEncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    private readonly IShortAliasGenerator _shortAliasGenerator = shortAliasGenerator ?? throw new ArgumentNullException(nameof(shortAliasGenerator));
    private readonly IMessagePublisher _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IBlockedDomainChecker _blockedDomainChecker = blockedDomainChecker ?? throw new ArgumentNullException(nameof(blockedDomainChecker));
    private readonly IStructuredLogger<CreateShortUrlCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<CreateShortUrlResult> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var appContext = ResolverAppContext.Current;
        var userId = appContext.Headers.TryGetValue(Constants.Headers.UserId, out var configuredUserId)
            ? configuredUserId
            : "anonymous";

        _logger.LogInformation(
            "CreateShortUrl command handling started RequestId {RequestId} CorrelationId {CorrelationId} UserId {UserId} Alias {Alias}",
            appContext.RequestId,
            appContext.CorrelationId,
            userId,
            request.Alias ?? "generated");

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

        _logger.LogInformation(
            "CreateShortUrl persisted entity RequestId {RequestId} CorrelationId {CorrelationId} EntityId {EntityId} Alias {Alias}",
            appContext.RequestId,
            appContext.CorrelationId,
            link.Id.Value,
            linkAlias.Alias);

        await PublishDomainEventsAsync(link.DomainEvents, cancellationToken);
        link.ClearDomainEvents();

        await _cacheService.SetAsync(
            LinkCacheKeys.ResolveDestinationByAlias(linkAlias.Alias),
            originalUrl,
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = LinkCachingDefaults.DefaultCacheDuration
            },
             cancellationToken);

        _logger.LogInformation(
            "CreateShortUrl command handling completed RequestId {RequestId} CorrelationId {CorrelationId} EntityId {EntityId} Alias {Alias}",
            appContext.RequestId,
            appContext.CorrelationId,
            link.Id.Value,
            linkAlias.Alias);

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
                PublishLinkCreatedEventAsync(linkCreatedDomainEvent, cancellationToken),
            _ => Task.CompletedTask
        });

        return Task.WhenAll(publishTasks);
    }

    private async Task PublishLinkCreatedEventAsync(
        LinkCreatedDomainEvent linkCreatedDomainEvent,
        CancellationToken cancellationToken)
    {
        var appContext = ResolverAppContext.Current;
        _logger.LogInformation(
            "Publishing domain event {DomainEventType} RequestId {RequestId} CorrelationId {CorrelationId} EntityId {EntityId}",
            nameof(LinkCreatedDomainEvent),
            appContext.RequestId,
            appContext.CorrelationId,
            linkCreatedDomainEvent.LinkId.Value);

        await _messagePublisher.PublishAsync(
            new LinkCreatedMessage(
                linkCreatedDomainEvent.LinkId.Value,
                linkCreatedDomainEvent.TenantId.Value,
                linkCreatedDomainEvent.DestinationUrl,
                linkCreatedDomainEvent.OccurredOnUtc),
            cancellationToken);
    }
}
