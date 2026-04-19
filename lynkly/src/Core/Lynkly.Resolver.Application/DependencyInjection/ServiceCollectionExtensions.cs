using Lynkly.Resolver.Application.Abstractions;
using Lynkly.Resolver.Application.BlockedDomains;
using Lynkly.Shared.Kernel.MediatR.Extensions;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Application.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddResolverApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLynklyMediator(typeof(ModuleRegistration).Assembly);
        services.AddOptions<AliasGeneratorOptions>()
            .BindConfiguration(AliasGeneratorOptions.SectionName);
        services.AddSingleton<IShortAliasGenerator, HmacShortAliasGenerator>();
        services.AddOptions<BlockedDomainOptions>()
            .BindConfiguration(BlockedDomainOptions.SectionName);
        services.AddSingleton<IBlockedDomainChecker, ConfigurableBlockedDomainChecker>();

        return services;
    }
}
