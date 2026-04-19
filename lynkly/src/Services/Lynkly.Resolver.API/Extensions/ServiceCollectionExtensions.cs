using FluentValidation;
using Lynkly.Resolver.Application.DependencyInjection;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Resolver.Infrastructure.DependencyInjection;
using Lynkly.Shared.Kernel.Core.Web;
using Lynkly.Shared.Kernel.Security.Extensions;
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

        services.AddSecurity(configuration);
        services.AddResolverApplication();
        services.AddValidatorsFromAssemblyContaining<CreateShortUrlCommandValidator>(includeInternalTypes: true);
        services.AddResolverInfrastructure(configuration);

        return services;
    }
}
