using Lynkly.Resolver.Application.DependencyInjection;
using Lynkly.Resolver.Infrastructure.DependencyInjection;
using Lynkly.Shared.Kernel.Core.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Lynkly.Resolver.API.Extensions;

public static class ModuleRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSwaggerSupport(configuration);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddAuthorization();

        services.AddRequestContextSupport();

        services.AddResolverApplication();
        services.AddResolverInfrastructure();

        return services;
    }
}
