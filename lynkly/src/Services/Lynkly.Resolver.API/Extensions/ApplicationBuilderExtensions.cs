using Lynkly.Resolver.API.Middlewares;
using Lynkly.Shared.Kernel.Core.Context;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lynkly.Resolver.API.Extensions;

public static class ApplicationBuilderExtensions
{
  public static IServiceCollection AddRequestContextSupport(this IServiceCollection services)
  {
    services.TryAddEnumerable(ServiceDescriptor.Singleton<IRequestContextEnricher, DefaultRequestContextEnricher>());
    return services;
  }

  public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
  {
    return app.UseMiddleware<RequestContextMiddleware>();
  }
}
