using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Caching.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lynkly.Shared.Kernel.Caching.Providers;

internal sealed class CompositeCacheService : ICacheService
{
    private readonly IReadOnlyList<ICacheProvider> _providers;
    private readonly CacheServiceRegistrationOptions _registrationOptions;
    private readonly SemaphoreSlim[] _keyLocks = CreateKeyLocks();
    private readonly ILogger<CompositeCacheService> _logger;

    public CompositeCacheService(
        IEnumerable<ICacheProvider> providers,
        CacheServiceRegistrationOptions registrationOptions,
        ILogger<CompositeCacheService>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(registrationOptions);

        _providers = providers.Where(provider => provider.IsAvailable).ToArray();
        if (_providers.Count == 0)
        {
            throw new InvalidOperationException("No available cache providers were registered.");
        }

        _registrationOptions = registrationOptions;
        _logger = logger ?? NullLogger<CompositeCacheService>.Instance;
    }

    public async Task<TValue?> GetAsync<TValue>(
        CacheKey<TValue> key,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < _providers.Count; i++)
        {
            var provider = _providers[i];
            TValue? value;

            try
            {
                value = await provider.GetAsync<TValue>(key.Value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cache provider {CacheProvider} failed while reading a cache entry.", provider.Name);
                continue;
            }

            if (value is null)
            {
                continue;
            }

            if (_registrationOptions.BackfillEarlierProvidersOnReadHit && i > 0)
            {
                var tasks = _providers
                    .Take(i)
                    .Select(async previousProvider =>
                    {
                        try
                        {
                            await previousProvider.SetAsync(
                                key.Value,
                                value,
                                _registrationOptions.DefaultEntryOptions,
                                CancellationToken.None);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogWarning(
                                exception,
                                "Cache provider {CacheProvider} failed while backfilling a cache entry.",
                                previousProvider.Name);
                        }
                    });

                await Task.WhenAll(tasks);
            }

            return value;
        }

        return default;
    }

    public async Task SetAsync<TValue>(
        CacheKey<TValue> key,
        TValue value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var effectiveOptions = options ?? _registrationOptions.DefaultEntryOptions;
        foreach (var provider in _providers)
        {
            try
            {
                await provider.SetAsync(key.Value, value, effectiveOptions, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cache provider {CacheProvider} failed while writing a cache entry.", provider.Name);
            }
        }
    }

    public async Task RemoveAsync<TValue>(
        CacheKey<TValue> key,
        CancellationToken cancellationToken = default)
    {
        var tasks = _providers.Select(provider => provider.RemoveAsync(key.Value, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task<TValue> GetOrCreateAsync<TValue>(
        CacheKey<TValue> key,
        Func<CancellationToken, Task<TValue>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var cached = await GetAsync(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var keyLock = GetKeyLock(key.Value);
        await keyLock.WaitAsync(cancellationToken);

        try
        {
            cached = await GetAsync(key, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var created = await factory(cancellationToken);
            ArgumentNullException.ThrowIfNull(created);

            await SetAsync(key, created, options, cancellationToken);
            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    private static SemaphoreSlim[] CreateKeyLocks()
    {
        var locks = new SemaphoreSlim[64];
        for (var i = 0; i < locks.Length; i++)
        {
            locks[i] = new SemaphoreSlim(1, 1);
        }

        return locks;
    }

    private SemaphoreSlim GetKeyLock(string key)
    {
        var index = (int)(unchecked((uint)key.GetHashCode(StringComparison.Ordinal)) % (uint)_keyLocks.Length);
        return _keyLocks[index];
    }
}
