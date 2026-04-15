using Microsoft.AspNetCore.Http;

namespace Lynkly.Shared.Kernel.Context.Abstractions;

public interface IRequestContextEnricher
{
    void EnrichRequest(HttpContext httpContext, AppContext appContext);

    void EnrichResponse(HttpContext httpContext, AppContext appContext);
}
