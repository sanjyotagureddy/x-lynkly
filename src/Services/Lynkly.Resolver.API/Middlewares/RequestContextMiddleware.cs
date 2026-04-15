using Lynkly.Shared.Kernel.Context;
using Microsoft.Extensions.Hosting;

namespace Lynkly.Resolver.API.Middlewares;

internal sealed class RequestContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IEnumerable<IRequestContextEnricher> enrichers)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var appContext = AppContext.FromHttpContext(httpContext, environment.ApplicationName);

        foreach (var enricher in enrichers)
        {
            enricher.EnrichRequest(httpContext, appContext);
        }

        using (RequestContextScope.BeginScope(appContext))
        {
            httpContext.Response.OnStarting(() =>
            {
                foreach (var enricher in enrichers)
                {
                    enricher.EnrichResponse(httpContext, appContext);
                }

                return Task.CompletedTask;
            });

            await next(httpContext);
        }
    }
}
