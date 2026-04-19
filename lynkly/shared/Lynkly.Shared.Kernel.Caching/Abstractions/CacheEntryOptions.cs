namespace Lynkly.Shared.Kernel.Caching.Abstractions;

public sealed record CacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; init; }

    public TimeSpan? SlidingExpiration { get; init; }
}
