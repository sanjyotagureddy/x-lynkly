using System.Text;
using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Application.UseCases.Links;
using Lynkly.Resolver.Application.UseCases.Links.ResolveShortUrl;
using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Core.Helpers.Security;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;
using NSubstitute;

namespace Lynkly.Resolver.UnitTests.Application.Links.ResolveShortUrl;

public sealed class ResolveShortUrlQueryHandlerTests
{
    [Fact]
    public async Task Handle_UsesDefaultThreeMinuteCacheDuration_WhenHeaderIsMissing()
    {
        var repository = Substitute.For<ILinkReadRepository>();
        repository.GetEncryptedDestinationByAliasAsync("summer-sale", Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(SecurityHelper.ToBase64(Encoding.UTF8.GetBytes("encrypted")));

        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Decrypt(Arg.Any<byte[]>()).Returns(Encoding.UTF8.GetBytes("https://example.com/summer"));

        var cacheService = Substitute.For<ICacheService>();
        cacheService.GetAsync(Arg.Any<CacheKey<string>>(), Arg.Any<CancellationToken>()).Returns((string?)null);

        var logger = Substitute.For<IStructuredLogger<ResolveShortUrlQueryHandler>>();
        var handler = new ResolveShortUrlQueryHandler(repository, encryptionService, cacheService, logger);

        var result = await handler.Handle(new ResolveShortUrlQuery("summer-sale", null), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("https://example.com/summer", result!.DestinationUrl);
        await cacheService.Received(1).SetAsync(
            Arg.Any<CacheKey<string>>(),
            "https://example.com/summer",
            Arg.Is<CacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == LinkCachingDefaults.DefaultCacheDuration),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsesHeaderBasedCacheDurationOverride_WhenProvided()
    {
        var repository = Substitute.For<ILinkReadRepository>();
        repository.GetEncryptedDestinationByAliasAsync("promo", Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(SecurityHelper.ToBase64(Encoding.UTF8.GetBytes("encrypted")));

        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Decrypt(Arg.Any<byte[]>()).Returns(Encoding.UTF8.GetBytes("https://example.com/promo"));

        var cacheService = Substitute.For<ICacheService>();
        cacheService.GetAsync(Arg.Any<CacheKey<string>>(), Arg.Any<CancellationToken>()).Returns((string?)null);

        var logger = Substitute.For<IStructuredLogger<ResolveShortUrlQueryHandler>>();
        var handler = new ResolveShortUrlQueryHandler(repository, encryptionService, cacheService, logger);

        await handler.Handle(new ResolveShortUrlQuery("promo", 45), CancellationToken.None);

        await cacheService.Received(1).SetAsync(
            Arg.Any<CacheKey<string>>(),
            "https://example.com/promo",
            Arg.Is<CacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == TimeSpan.FromSeconds(45)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsCachedValue_WithoutRepositoryLookup()
    {
        var repository = Substitute.For<ILinkReadRepository>();
        var encryptionService = Substitute.For<IEncryptionService>();
        var cacheService = Substitute.For<ICacheService>();
        cacheService.GetAsync(Arg.Any<CacheKey<string>>(), Arg.Any<CancellationToken>())
            .Returns("https://cached.example/path");

        var logger = Substitute.For<IStructuredLogger<ResolveShortUrlQueryHandler>>();
        var handler = new ResolveShortUrlQueryHandler(repository, encryptionService, cacheService, logger);

        var result = await handler.Handle(new ResolveShortUrlQuery("cached", null), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("https://cached.example/path", result!.DestinationUrl);
        await repository.DidNotReceive().GetEncryptedDestinationByAliasAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        encryptionService.DidNotReceive().Decrypt(Arg.Any<byte[]>());
    }

    [Fact]
    public async Task Handle_LogsStructuredAliasAndCorrelationProperties()
    {
        var repository = Substitute.For<ILinkReadRepository>();
        repository.GetEncryptedDestinationByAliasAsync("promo", Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(SecurityHelper.ToBase64(Encoding.UTF8.GetBytes("encrypted")));

        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Decrypt(Arg.Any<byte[]>()).Returns(Encoding.UTF8.GetBytes("https://example.com/promo"));

        var cacheService = Substitute.For<ICacheService>();
        cacheService.GetAsync(Arg.Any<CacheKey<string>>(), Arg.Any<CancellationToken>()).Returns((string?)null);

        var logger = Substitute.For<IStructuredLogger<ResolveShortUrlQueryHandler>>();
        var handler = new ResolveShortUrlQueryHandler(repository, encryptionService, cacheService, logger);

        await handler.Handle(new ResolveShortUrlQuery("promo", null), CancellationToken.None);

        logger.Received().LogInformation(
            "ResolveShortUrl query handling started RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
            Arg.Any<object?[]>());
        logger.Received().LogInformation(
            "ResolveShortUrl query handling completed RequestId {RequestId} CorrelationId {CorrelationId} Alias {Alias}",
            Arg.Any<object?[]>());
    }
}
