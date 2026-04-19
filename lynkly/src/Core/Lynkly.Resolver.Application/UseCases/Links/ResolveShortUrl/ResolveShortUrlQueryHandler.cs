using System.Text;
using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Application.UseCases.Links;
using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Core;
using Lynkly.Shared.Kernel.Core.Helpers.Security;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;
using ResolverAppContext = Lynkly.Shared.Kernel.Core.AppContext;

namespace Lynkly.Resolver.Application.UseCases.Links.ResolveShortUrl;

public sealed class ResolveShortUrlQueryHandler(
    ILinkReadRepository repository,
    IEncryptionService encryptionService,
    ICacheService cacheService,
    IStructuredLogger<ResolveShortUrlQueryHandler> logger,
    TimeProvider? timeProvider = null)
    : IRequestHandler<ResolveShortUrlQuery, ResolveShortUrlResult?>
{
    private readonly ILinkReadRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IEncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IStructuredLogger<ResolveShortUrlQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<ResolveShortUrlResult?> Handle(ResolveShortUrlQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Alias);

        var normalizedAlias = request.Alias.Trim().ToLowerInvariant();
        var appContext = ResolverAppContext.Current;

        _logger.LogInformation(
            "ResolveShortUrl query handling started RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
            appContext.RequestId,
            appContext.CorrelationId,
            normalizedAlias);

        var cacheKey = LinkCacheKeys.ResolveDestinationByAlias(normalizedAlias);
        var cachedDestination = await _cacheService.GetAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedDestination))
        {
            _logger.LogInformation(
                "ResolveShortUrl cache hit RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
                appContext.RequestId,
                appContext.CorrelationId,
                normalizedAlias);
            return new ResolveShortUrlResult(cachedDestination);
        }

        _logger.LogInformation(
            "ResolveShortUrl cache miss RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
            appContext.RequestId,
            appContext.CorrelationId,
            normalizedAlias);

        var encryptedDestination = await _repository.GetEncryptedDestinationByAliasAsync(
            normalizedAlias,
            _timeProvider.GetUtcNow(),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(encryptedDestination))
        {
            _logger.LogWarning(
                "ResolveShortUrl query returned no destination RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
                appContext.RequestId,
                appContext.CorrelationId,
                normalizedAlias);
            return null;
        }

        var decryptedDestination = Encoding.UTF8.GetString(
            _encryptionService.Decrypt(SecurityHelper.FromBase64ToBytes(encryptedDestination)));

        var cacheDuration = request.CacheDurationSeconds is > 0
            ? TimeSpan.FromSeconds(request.CacheDurationSeconds.Value)
            : LinkCachingDefaults.DefaultCacheDuration;

        await _cacheService.SetAsync(
            cacheKey,
            decryptedDestination,
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration
            },
             cancellationToken);

        _logger.LogInformation(
            "ResolveShortUrl query handling completed RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
            appContext.RequestId,
            appContext.CorrelationId,
            normalizedAlias);

        return new ResolveShortUrlResult(decryptedDestination);
    }
}
