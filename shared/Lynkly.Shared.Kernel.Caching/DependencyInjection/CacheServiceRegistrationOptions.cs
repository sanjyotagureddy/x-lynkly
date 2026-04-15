using Lynkly.Shared.Kernel.Caching.Abstractions;

namespace Lynkly.Shared.Kernel.Caching.DependencyInjection;

public sealed class CacheServiceRegistrationOptions
{
    public bool EnableInMemoryProvider { get; init; } = true;

    public bool EnableDistributedProvider { get; init; } = true;

    public CacheReadPreference ReadPreference { get; init; } = CacheReadPreference.PreferInMemory;

    public bool BackfillEarlierProvidersOnReadHit { get; init; } = true;

    public CacheEntryOptions DefaultEntryOptions { get; init; } = new();
}
