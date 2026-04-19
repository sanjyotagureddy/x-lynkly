using Lynkly.Shared.Kernel.Caching.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Diagnostics;

namespace Lynkly.Resolver.Infrastructure.Caching.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string RedisConnectionStringName = "lynkly-redis";

    public static IServiceCollection AddResolverCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var redisConnectionString = configuration.GetConnectionString(RedisConnectionStringName);
        var redisAvailable = IsRedisAvailable(redisConnectionString);
        if (!redisAvailable)
        {
            Trace.TraceWarning("Redis cache is unavailable or not configured. Falling back to in-memory cache provider.");
        }

        if (redisAvailable)
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString!);
        }

        services.AddKernelCaching(options =>
        {
            options.ReadPreference = redisAvailable
                ? Lynkly.Shared.Kernel.Caching.Abstractions.CacheReadPreference.PreferDistributed
                : Lynkly.Shared.Kernel.Caching.Abstractions.CacheReadPreference.PreferInMemory;
        });

        return services;
    }

    private static bool IsRedisAvailable(string? redisConnectionString)
    {
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            return false;
        }

        try
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = true;
            options.ConnectRetry = 0;
            options.ConnectTimeout = 1000;
            options.SyncTimeout = 1000;

            using var multiplexer = ConnectionMultiplexer.Connect(options);
            return multiplexer.IsConnected;
        }
        catch (Exception exception)
        {
            Trace.TraceWarning($"Redis validation failed: {exception.Message}");
            return false;
        }
    }
}
