using Lynkly.Shared.Kernel.Context.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Lynkly.Shared.Kernel.Context.Enrichers;

internal sealed class CorrelationIdRequestContextEnricher : IRequestContextEnricher
{
    public void EnrichRequest(HttpContext httpContext, AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(appContext);

        httpContext.Request.Headers[AppContext.CorrelationIdHeaderName] = appContext.CorrelationId;
    }

    public void EnrichResponse(HttpContext httpContext, AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(appContext);

        if (!httpContext.Response.Headers.ContainsKey(AppContext.CorrelationIdHeaderName))
        {
            httpContext.Response.Headers[AppContext.CorrelationIdHeaderName] = appContext.CorrelationId;
        }
    }
}
