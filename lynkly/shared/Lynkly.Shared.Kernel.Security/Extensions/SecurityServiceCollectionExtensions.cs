using Lynkly.Shared.Kernel.Security.Authentication;
using Lynkly.Shared.Kernel.Security.Authorization;
using Lynkly.Shared.Kernel.Security.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Shared.Kernel.Security.Extensions;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<ISecurityService, NoOpSecurityService>();
        services.TryAddSingleton<ITokenService, NoOpTokenService>();
        services.TryAddSingleton<IUserContext, NoOpUserContext>();

        return services;
    }
}
