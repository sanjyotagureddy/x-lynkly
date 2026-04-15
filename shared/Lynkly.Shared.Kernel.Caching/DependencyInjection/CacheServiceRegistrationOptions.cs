using Lynkly.Shared.Kernel.Caching.Abstractions;

namespace Lynkly.Shared.Kernel.Caching.DependencyInjection;

public sealed class CacheServiceRegistrationOptions
{
    public bool EnableInMemoryProvider { get; set; } = true;

    public bool EnableDistributedProvider { get; set; } = true;

    public CacheReadPreference ReadPreference { get; set; } = CacheReadPreference.PreferInMemory;

    public bool BackfillEarlierProvidersOnReadHit { get; set; } = true;

    public CacheEntryOptions DefaultEntryOptions { get; set; } = new();
}
