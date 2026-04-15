using Lynkly.Shared.Kernel.Context;

namespace Lynkly.Resolver.API.Middlewares;

internal interface IRequestContextEnricher
{
    void EnrichRequest(HttpContext httpContext, AppContext appContext);

    void EnrichResponse(HttpContext httpContext, AppContext appContext);
}
