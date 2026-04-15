using Lynkly.Resolver.API.Middlewares;
using Lynkly.Shared.Kernel.Context.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Resolver.API.Extensions;

public static class RequestContextExtensions
{
    public static IServiceCollection AddRequestContextSupport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKernelContext();
        services.TryAddSingleton<RequestContextMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseRequestContextSupport(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<RequestContextMiddleware>();
    }
}
