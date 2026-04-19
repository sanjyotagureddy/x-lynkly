using Lynkly.Shared.Kernel.MediatR.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Application.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddResolverApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLynklyMediator(typeof(ModuleRegistration).Assembly);

        return services;
    }
}
