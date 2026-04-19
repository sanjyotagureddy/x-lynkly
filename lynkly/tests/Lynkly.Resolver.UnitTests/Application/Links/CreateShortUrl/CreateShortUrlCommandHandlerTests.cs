using System.Text;
using Lynkly.Resolver.Application.Abstractions;
using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Resolver.Domain.Links;
using Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;
using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;
using NSubstitute;

namespace Lynkly.Resolver.UnitTests.Application.Links.CreateShortUrl;

public sealed class CreateShortUrlCommandHandlerTests
{
    private static CreateShortUrlCommandHandler BuildHandler(
        InMemoryLinkWriteRepository? repository = null,
        TestEncryptionService? encryption = null,
        IShortAliasGenerator? aliasGenerator = null,
        IBlockedDomainChecker? blockedDomainChecker = null,
        TimeProvider? timeProvider = null)
    {
        return new CreateShortUrlCommandHandler(
            repository ?? new InMemoryLinkWriteRepository(),
            encryption ?? new TestEncryptionService(),
            aliasGenerator ?? new TestShortAliasGenerator("generated-slug"),
            Substitute.For<IMessagePublisher>(),
            blockedDomainChecker ?? new AllowAllDomainChecker(),
            timeProvider);
    }

    [Fact]
    public async Task Handle_EncryptsAndPersistsDestinationUrl_AndPublishesMessage()
    {
        var repository = new InMemoryLinkWriteRepository();
        var encryptionService = new TestEncryptionService();
        var aliasGenerator = new TestShortAliasGenerator("generated-slug");
        var messagePublisher = Substitute.For<IMessagePublisher>();
        var handler = new CreateShortUrlCommandHandler(
            repository, encryptionService, aliasGenerator, messagePublisher,
            new AllowAllDomainChecker());

        const string originalUrl = "https://example.com/some/long/path";
        var command = new CreateShortUrlCommand(originalUrl, "summer-sale", DateTimeOffset.UtcNow.AddDays(2));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("summer-sale", result.Alias);
        Assert.NotNull(repository.StoredLink);
        Assert.NotEqual(originalUrl, repository.StoredLink!.DestinationUrl);

        var decryptedBytes = encryptionService.Decrypt(Convert.FromBase64String(repository.StoredLink.DestinationUrl));
        Assert.Equal(originalUrl, Encoding.UTF8.GetString(decryptedBytes));

        await messagePublisher.Received(1).PublishAsync(
            Arg.Is<LinkCreatedMessage>(message =>
                message.LinkId == result.LinkId &&
                message.TenantId == repository.DefaultTenantId.Value &&
                message.EncryptedDestinationUrl == repository.StoredLink.DestinationUrl),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GeneratesAliasFromUrlAndTenant_AndRetriesOnCollision()
    {
        var repository = new InMemoryLinkWriteRepository();
        repository.ExistingAliases.Add("collision-slug");

        var aliasGenerator = new TestShortAliasGenerator("collision-slug", "final-slug");
        var handler = new CreateShortUrlCommandHandler(
            repository,
            new TestEncryptionService(),
            aliasGenerator,
            Substitute.For<IMessagePublisher>(),
            new AllowAllDomainChecker());

        var command = new CreateShortUrlCommand("https://example.com/landing", null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("final-slug", result.Alias);
        Assert.Collection(
            aliasGenerator.Calls,
            call =>
            {
                Assert.Equal(repository.DefaultTenantId, call.TenantId);
                Assert.Equal("https://example.com/landing", call.OriginalUrl);
                Assert.Equal(0, call.Attempt);
            },
            call =>
            {
                Assert.Equal(repository.DefaultTenantId, call.TenantId);
                Assert.Equal("https://example.com/landing", call.OriginalUrl);
                Assert.Equal(1, call.Attempt);
            });
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenAliasAlreadyExists()
    {
        var repository = new InMemoryLinkWriteRepository();
        repository.ExistingAliases.Add("existing");

        var handler = new CreateShortUrlCommandHandler(
            repository,
            new TestEncryptionService(),
            new TestShortAliasGenerator("unused"),
            Substitute.For<IMessagePublisher>(),
            new AllowAllDomainChecker());

        var command = new CreateShortUrlCommand("https://example.com", "existing", null);

        await Assert.ThrowsAsync<AliasAlreadyExistsException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsBlockedDomain_WhenUrlDomainIsBlocked()
    {
        var handler = BuildHandler(blockedDomainChecker: new BlockListDomainChecker("blocked.com"));

        var command = new CreateShortUrlCommand("https://blocked.com/some/path", null, null);

        await Assert.ThrowsAsync<BlockedDomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Succeeds_WhenUrlDomainIsNotBlocked()
    {
        var handler = BuildHandler(blockedDomainChecker: new BlockListDomainChecker("other.com"));

        var command = new CreateShortUrlCommand("https://allowed.com/path", null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result.Alias);
    }

    [Fact]
    public async Task Handle_DefaultsExpiryToOneMonth_WhenNotProvided()
    {
        var fixedNow = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(fixedNow);
        var repository = new InMemoryLinkWriteRepository();
        var handler = BuildHandler(repository: repository, timeProvider: fakeTime);

        var command = new CreateShortUrlCommand("https://example.com/path", null, null);
        await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(repository.StoredLink);
        Assert.NotNull(repository.StoredLink!.ExpiresAtUtc);
        Assert.Equal(fixedNow.AddDays(30), repository.StoredLink.ExpiresAtUtc!.Value);
    }

    [Fact]
    public async Task Handle_UsesProvidedExpiry_WhenExplicitlySet()
    {
        var fixedNow = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(fixedNow);
        var repository = new InMemoryLinkWriteRepository();
        var handler = BuildHandler(repository: repository, timeProvider: fakeTime);

        var explicitExpiry = fixedNow.AddDays(7);
        var command = new CreateShortUrlCommand("https://example.com/path", null, explicitExpiry);
        await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(repository.StoredLink);
        Assert.Equal(explicitExpiry, repository.StoredLink!.ExpiresAtUtc!.Value);
    }

    // ---- test doubles ----

    private sealed class InMemoryLinkWriteRepository : ILinkWriteRepository
    {
        public TenantId DefaultTenantId { get; } = new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        public HashSet<string> ExistingAliases { get; } = new(StringComparer.Ordinal);
        public Link? StoredLink { get; private set; }
        public LinkAlias? StoredAlias { get; private set; }

        public Task<TenantId> GetOrCreateDefaultTenantIdAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(DefaultTenantId);
        }

        public Task<bool> AliasExistsAsync(TenantId tenantId, string alias, CancellationToken cancellationToken)
        {
            return Task.FromResult(ExistingAliases.Contains(alias));
        }

        public void Add(Link link, LinkAlias linkAlias)
        {
            StoredLink = link;
            StoredAlias = linkAlias;
            ExistingAliases.Add(linkAlias.Alias);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestEncryptionService : IEncryptionService
    {
        public byte[] Encrypt(string input)
        {
            return Encrypt(Encoding.UTF8.GetBytes(input));
        }

        public byte[] Encrypt(byte[] input)
        {
            return [.. input.Reverse()];
        }

        public byte[] Encrypt(string input, string tenantId)
        {
            return Encrypt(input);
        }

        public byte[] Encrypt(byte[] input, string tenantId)
        {
            return Encrypt(input);
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            return [.. encryptedData.Reverse()];
        }
    }

    private sealed class TestShortAliasGenerator(params string[] aliases) : IShortAliasGenerator
    {
        private readonly string[] _aliases = aliases;
        public List<(TenantId TenantId, string OriginalUrl, int Attempt)> Calls { get; } = [];

        public string Generate(TenantId tenantId, string originalUrl, int attempt)
        {
            ArgumentNullException.ThrowIfNull(originalUrl);
            ArgumentOutOfRangeException.ThrowIfNegative(attempt);

            Calls.Add((tenantId, originalUrl, attempt));

            if (attempt < _aliases.Length)
            {
                return _aliases[attempt];
            }

            return _aliases[^1];
        }
    }

    private sealed class AllowAllDomainChecker : IBlockedDomainChecker
    {
        public bool IsBlocked(string host) => false;
    }

    private sealed class BlockListDomainChecker(params string[] blocked) : IBlockedDomainChecker
    {
        private readonly HashSet<string> _blocked = new(blocked, StringComparer.OrdinalIgnoreCase);

        public bool IsBlocked(string host) => _blocked.Contains(host);
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
