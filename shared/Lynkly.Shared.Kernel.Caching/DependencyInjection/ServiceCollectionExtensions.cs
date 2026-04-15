using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Caching.Providers;
using Lynkly.Shared.Kernel.Caching.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Shared.Kernel.Caching.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelCaching(
        this IServiceCollection services,
        Action<CacheServiceRegistrationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var registrationOptions = new CacheServiceRegistrationOptions();
        configure?.Invoke(registrationOptions);

        if (!registrationOptions.EnableInMemoryProvider && !registrationOptions.EnableDistributedProvider)
        {
            throw new InvalidOperationException("At least one cache provider must be enabled.");
        }

        if (!registrationOptions.EnableInMemoryProvider
            && registrationOptions.EnableDistributedProvider
            && !services.Any(descriptor => descriptor.ServiceType == typeof(IDistributedCache)))
        {
            throw new InvalidOperationException(
                "Distributed caching is enabled without in-memory fallback, but no IDistributedCache is registered. " +
                "Register IDistributedCache before calling AddKernelCaching or enable the in-memory provider.");
        }

        services.TryAddSingleton(registrationOptions);
        services.TryAddSingleton<ICacheSerializer, JsonCacheSerializer>();

        var addDistributedFirst = registrationOptions.ReadPreference == CacheReadPreference.PreferDistributed;

        if (addDistributedFirst)
        {
            RegisterDistributedProvider(services, registrationOptions);
            RegisterInMemoryProvider(services, registrationOptions);
        }
        else
        {
            RegisterInMemoryProvider(services, registrationOptions);
            RegisterDistributedProvider(services, registrationOptions);
        }

        services.TryAddSingleton<ICacheService, CompositeCacheService>();

        return services;
    }

    private static void RegisterInMemoryProvider(
        IServiceCollection services,
        CacheServiceRegistrationOptions options)
    {
        if (!options.EnableInMemoryProvider)
        {
            return;
        }

        services.AddMemoryCache();

        services.AddSingleton<ICacheProvider>(serviceProvider =>
            new InMemoryCacheProvider(
                serviceProvider.GetRequiredService<IMemoryCache>()));
    }

    private static void RegisterDistributedProvider(
        IServiceCollection services,
        CacheServiceRegistrationOptions options)
    {
        if (!options.EnableDistributedProvider)
        {
            return;
        }

        services.AddSingleton<ICacheProvider>(serviceProvider =>
            new DistributedCacheProvider(
                serviceProvider.GetService<IDistributedCache>(),
                serviceProvider.GetRequiredService<ICacheSerializer>()));
    }
}
