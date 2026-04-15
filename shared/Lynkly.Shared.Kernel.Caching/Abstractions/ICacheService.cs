namespace Lynkly.Shared.Kernel.Caching.Abstractions;

public interface ICacheService
{
    Task<TValue?> GetAsync<TValue>(
        CacheKey<TValue> key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a non-null value in all available providers.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    Task SetAsync<TValue>(
        CacheKey<TValue> key,
        TValue value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync<TValue>(
        CacheKey<TValue> key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the value from cache or creates and stores a non-null value when missing.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is null or when the factory returns null.
    /// </exception>
    Task<TValue> GetOrCreateAsync<TValue>(
        CacheKey<TValue> key,
        Func<CancellationToken, Task<TValue>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);
}
