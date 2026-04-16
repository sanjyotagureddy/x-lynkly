using Lynkly.Shared.Kernel.Context;
using Lynkly.Shared.Kernel.Core;

namespace Lynkly.Resolver.API.Middlewares;

internal sealed class CorrelationIdRequestContextEnricher : IRequestContextEnricher
{
    public void EnrichRequest(HttpContext httpContext, AppCallContext appCallContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(appCallContext);

        if (!string.IsNullOrWhiteSpace(appCallContext.CorrelationId))
        {
            return;
        }

        if (httpContext.Request.Headers.TryGetValue(Constants.Headers.CorrelationId, out var correlationHeader)
            && !string.IsNullOrWhiteSpace(correlationHeader.ToString()))
        {
            appCallContext.CorrelationId = correlationHeader.ToString();
            return;
        }

        appCallContext.CorrelationId = Guid.NewGuid().ToString("N");
    }

    public void EnrichResponse(HttpContext httpContext, AppCallContext appCallContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(appCallContext);

        if (!string.IsNullOrWhiteSpace(appCallContext.CorrelationId))
        {
            httpContext.Response.Headers[Constants.Headers.CorrelationId] = appCallContext.CorrelationId;
        }
    }
}
