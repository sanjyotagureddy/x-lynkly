namespace Lynkly.Shared.Kernel.Caching.Abstractions;

public interface ICacheProvider
{
    string Name { get; }

    bool IsAvailable { get; }

    Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<TValue>(
        string key,
        TValue value,
        CacheEntryOptions options,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
