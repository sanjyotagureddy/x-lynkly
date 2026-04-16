using Lynkly.Shared.Kernel.Context;

namespace Lynkly.Resolver.API.Middlewares;

internal interface IRequestContextEnricher
{
    void EnrichRequest(HttpContext httpContext, AppCallContext appCallContext);

    void EnrichResponse(HttpContext httpContext, AppCallContext appCallContext);
}
