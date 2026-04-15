using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Caching.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Caching;

public sealed class CacheServiceTests
{
    [Fact]
    public void CacheKey_Should_Throw_For_Whitespace_Value()
    {
        Assert.Throws<ArgumentException>(() => new CacheKey<string>(" "));
    }

    [Fact]
    public async Task AddKernelCaching_Should_Fallback_To_InMemory_When_Distributed_Is_Not_Configured()
    {
        var services = new ServiceCollection();
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:abc");

        await cache.SetAsync(key, "https://example.com");
        var value = await cache.GetAsync(key);

        Assert.Equal("https://example.com", value);
    }

    [Fact]
    public async Task AddKernelCaching_Should_Write_To_Multiple_Providers_When_Distributed_Is_Configured()
    {
        var distributedCache = new FakeDistributedCache();

        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:def");

        await cache.SetAsync(key, "https://example.org");

        Assert.NotNull(await distributedCache.GetAsync("links:def"));
        Assert.Equal("https://example.org", await cache.GetAsync(key));
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_Use_Cache_After_First_Creation()
    {
        var services = new ServiceCollection();
        services.AddKernelCaching();

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:ghi");
        var invocationCount = 0;

        var first = await cache.GetOrCreateAsync(
            key,
            _ =>
            {
                invocationCount++;
                return Task.FromResult("https://lynk.ly");
            });

        var second = await cache.GetOrCreateAsync(
            key,
            _ =>
            {
                invocationCount++;
                return Task.FromResult("https://should-not-be-used");
            });

        Assert.Equal("https://lynk.ly", first);
        Assert.Equal("https://lynk.ly", second);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_Only_Invoke_Factory_Once_For_Concurrent_Calls()
    {
        var services = new ServiceCollection();
        services.AddKernelCaching();

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:concurrent");
        var invocationCount = 0;
        var factoryStarted = new ManualResetEventSlim(false);
        var releaseFactory = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<CancellationToken, Task<string>> factory = async _ =>
        {
            Interlocked.Increment(ref invocationCount);
            factoryStarted.Set();
            await releaseFactory.Task;
            return "https://concurrent.example";
        };

        var firstTask =
            cache.GetOrCreateAsync(
                key,
                factory);

        factoryStarted.Wait();

        var parallelTasks = Enumerable.Range(0, 7)
            .Select(_ => cache.GetOrCreateAsync(key, factory))
            .ToArray();

        releaseFactory.SetResult(true);

        var values = await Task.WhenAll(parallelTasks.Prepend(firstTask));

        Assert.All(values, value => Assert.Equal("https://concurrent.example", value));
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public void AddKernelCaching_Should_Throw_When_DistributedOnly_And_IDistributedCache_Is_Not_Registered()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddKernelCaching(options =>
            {
                options.EnableInMemoryProvider = false;
                options.EnableDistributedProvider = true;
            }));

        Assert.Contains("IDistributedCache", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_Should_Fallback_To_NextProvider_When_PreferredProvider_Throws()
    {
        var distributedCache = new FakeDistributedCache
        {
            ThrowOnGet = true
        };

        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:jkl");

        await cache.SetAsync(key, "https://fallback.example");
        var value = await cache.GetAsync(key);

        Assert.Equal("https://fallback.example", value);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Value_When_Backfill_Fails()
    {
        var distributedCache = new FakeDistributedCache
        {
            ThrowOnSet = true
        };

        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var key = new CacheKey<string>("links:mno");
        provider.GetRequiredService<IMemoryCache>().Set(key.Value, "https://memory-hit.example");
        var cache = provider.GetRequiredService<ICacheService>();

        var value = await cache.GetAsync(key);

        Assert.Equal("https://memory-hit.example", value);
    }

    [Fact]
    public async Task SetAsync_Should_Not_Throw_When_One_Provider_Fails()
    {
        var distributedCache = new FakeDistributedCache
        {
            ThrowOnSet = true
        };

        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:set-fallback");

        await cache.SetAsync(key, "https://set-fallback.example");

        var value = await cache.GetAsync(key);
        Assert.Equal("https://set-fallback.example", value);
    }

    [Fact]
    public async Task GetAsync_Should_Treat_Invalid_Distributed_Payload_As_Miss_And_Remove_Entry()
    {
        var distributedCache = new FakeDistributedCache();
        distributedCache.SetRaw("links:pqr", "not-json"u8.ToArray());

        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddKernelCaching(options =>
        {
            options.EnableInMemoryProvider = false;
            options.EnableDistributedProvider = true;
        });

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        var value = await cache.GetAsync(new CacheKey<string>("links:pqr"));

        Assert.Null(value);
        Assert.Null(await distributedCache.GetAsync("links:pqr"));
    }

    private sealed class FakeDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _store = new(StringComparer.Ordinal);
        public bool ThrowOnGet { get; set; }
        public bool ThrowOnSet { get; set; }

        public byte[]? Get(string key)
        {
            if (ThrowOnGet)
            {
                throw new InvalidOperationException("Simulated distributed cache get failure.");
            }

            return _store.TryGetValue(key, out var value) ? value : null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (ThrowOnSet)
            {
                throw new InvalidOperationException("Simulated distributed cache set failure.");
            }

            _store[key] = value;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _store.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void SetRaw(string key, byte[] value)
        {
            _store[key] = value;
        }
    }
}
