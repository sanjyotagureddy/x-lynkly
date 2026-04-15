using Lynkly.Shared.Kernel.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Lynkly.Shared.Kernel.Caching.Providers;

internal sealed class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheProvider(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public string Name => "in-memory";

    public bool IsAvailable => true;

    public Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(key, out TValue? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<TValue>(
        string key,
        TValue value,
        CacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        _memoryCache.Set(key, value, ToMemoryOptions(options));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    private static MemoryCacheEntryOptions ToMemoryOptions(CacheEntryOptions options)
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration
        };
    }
}
