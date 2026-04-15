using Lynkly.Shared.Kernel.Context;

namespace Lynkly.Resolver.API.Middlewares;

internal sealed class CorrelationIdRequestContextEnricher : IRequestContextEnricher
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public void EnrichRequest(HttpContext httpContext, AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(appContext);

        if (!string.IsNullOrWhiteSpace(appContext.CorrelationId))
        {
            return;
        }

        if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationHeader)
            && !string.IsNullOrWhiteSpace(correlationHeader.ToString()))
        {
            appContext.CorrelationId = correlationHeader.ToString();
            return;
        }

        appContext.CorrelationId = Guid.NewGuid().ToString("N");
    }

    public void EnrichResponse(HttpContext httpContext, AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(appContext);

        if (!string.IsNullOrWhiteSpace(appContext.CorrelationId))
        {
            httpContext.Response.Headers[CorrelationIdHeaderName] = appContext.CorrelationId;
        }
    }
}
