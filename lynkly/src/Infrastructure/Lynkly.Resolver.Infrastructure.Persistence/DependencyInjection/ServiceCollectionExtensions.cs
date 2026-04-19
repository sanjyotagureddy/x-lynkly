using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Infrastructure.Persistence.Internal;
using Lynkly.Shared.Kernel.Persistence.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Infrastructure.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("lynkly-db")
            ?? "Host=localhost;Database=lynkly_test";

        services.AddKernelPersistence();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ILinkWriteRepository, LinkWriteRepository>();

        return services;
    }
}
