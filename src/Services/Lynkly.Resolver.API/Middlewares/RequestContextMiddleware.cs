using Lynkly.Shared.Kernel.Context;
using Lynkly.Shared.Kernel.Context.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Lynkly.Resolver.API.Middlewares;

public sealed class RequestContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IEnumerable<IRequestContextEnricher> enrichers)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var appContext = Lynkly.Shared.Kernel.Context.AppContext.FromHttpContext(httpContext, environment.ApplicationName);

        foreach (var enricher in enrichers)
        {
            enricher.EnrichRequest(httpContext, appContext);
        }

        using (RequestContextScope.BeginScope(appContext))
        {
            var responseEnriched = 0;

            httpContext.Response.OnStarting(() =>
            {
                if (Interlocked.Exchange(ref responseEnriched, 1) == 0)
                {
                    foreach (var enricher in enrichers)
                    {
                        enricher.EnrichResponse(httpContext, appContext);
                    }
                }

                return Task.CompletedTask;
            });

            await next(httpContext);

            if (Interlocked.Exchange(ref responseEnriched, 1) == 0)
            {
                foreach (var enricher in enrichers)
                {
                    enricher.EnrichResponse(httpContext, appContext);
                }
            }
        }
    }
}
