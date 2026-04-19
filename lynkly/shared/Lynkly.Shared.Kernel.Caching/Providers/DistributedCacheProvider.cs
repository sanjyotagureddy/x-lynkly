using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Caching.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Lynkly.Shared.Kernel.Caching.Providers;

internal sealed class DistributedCacheProvider(
  IDistributedCache? distributedCache,
  ICacheSerializer serializer)
  : ICacheProvider
{
  private readonly ICacheSerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

    public string Name => "distributed";

    public bool IsAvailable => distributedCache is not null;

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        if (distributedCache is null)
        {
            return default;
        }

        var bytes = await distributedCache.GetAsync(key, cancellationToken);
        if (bytes is null)
        {
            return default;
        }

        try
        {
            return _serializer.Deserialize<TValue>(bytes);
        }
        catch (JsonException)
        {
            await TryRemoveCorruptedEntryAsync(key, cancellationToken);
            return default;
        }
        catch (NotSupportedException)
        {
            await TryRemoveCorruptedEntryAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<TValue>(
        string key,
        TValue value,
        CacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        if (distributedCache is null)
        {
            return;
        }

        var payload = _serializer.Serialize(value);
        await distributedCache.SetAsync(key, payload, ToDistributedOptions(options), cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (distributedCache is null)
        {
            return Task.CompletedTask;
        }

        return distributedCache.RemoveAsync(key, cancellationToken);
    }

    private async Task TryRemoveCorruptedEntryAsync(string key, CancellationToken cancellationToken)
    {
        if (distributedCache is null)
        {
            return;
        }

        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
        }
    }

    private static DistributedCacheEntryOptions ToDistributedOptions(CacheEntryOptions options)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration
        };
    }
}
