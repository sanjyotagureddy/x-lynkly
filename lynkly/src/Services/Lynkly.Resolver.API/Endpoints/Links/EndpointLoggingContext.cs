using Lynkly.Shared.Kernel.Core;
using Lynkly.Shared.Kernel.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.API.Endpoints.Links;

internal static class EndpointLoggingContext
{
    internal static IStructuredLogger<TEndpoint>? ResolveLogger<TEndpoint>(HttpContext httpContext)
    {
        return httpContext.RequestServices.GetService<IStructuredLogger<TEndpoint>>();
    }

    internal static string ResolveUserId(HttpContext httpContext)
    {
        var headerUserId = httpContext.Request.Headers[Constants.Headers.UserId].ToString();
        return httpContext.User.Identity?.Name
               ?? (string.IsNullOrWhiteSpace(headerUserId) ? "anonymous" : headerUserId);
    }
}
