using Lynkly.Shared.Kernel.Core.Context;
using Lynkly.Shared.Kernel.Core.Exceptions;

namespace Lynkly.Resolver.API.Middlewares;

public sealed class RequestContextMiddleware(RequestDelegate next, IHostEnvironment environment, IEnumerable<IRequestContextEnricher> enrichers)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var appContext = Shared.Kernel.Core.AppContext.FromHttpContext(httpContext, environment.ApplicationName);

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
