using System.Diagnostics;
using System.Security.Claims;
using Lynkly.Shared.Kernel.Context;
using Lynkly.Shared.Kernel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lynkly.Resolver.API.Middlewares;

internal sealed class RequestContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        // Resolve enrichers per-request so scoped/transient enrichers are supported
        // and ValidateScopes does not fail at startup.
        var enrichers = httpContext.RequestServices
            .GetServices<IRequestContextEnricher>()
            .ToArray();

        var appCallContext = BuildAppCallContext(httpContext, environment.ApplicationName);

        foreach (var enricher in enrichers)
        {
            enricher.EnrichRequest(httpContext, appCallContext);
        }

        using (RequestContextScope.BeginScope(appCallContext))
        {
            httpContext.Response.OnStarting(() =>
            {
                foreach (var enricher in enrichers)
                {
                    enricher.EnrichResponse(httpContext, appCallContext);
                }

                return Task.CompletedTask;
            });

            await next(httpContext);
        }
    }

    private static AppCallContext BuildAppCallContext(HttpContext httpContext, string applicationName)
    {
        var request = httpContext.Request;

        var correlationId = request.Headers.TryGetValue(Constants.Headers.CorrelationId, out var correlationValue)
                            && !string.IsNullOrWhiteSpace(correlationValue.ToString())
            ? correlationValue.ToString()
            : null;

        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var method = string.IsNullOrWhiteSpace(request.Method) ? HttpMethods.Get : request.Method;
        var path = request.Path.HasValue ? request.Path.Value! : "/";
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = httpContext.User.Identity?.Name;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = request.Headers.UserAgent.ToString();

        return AppCallContext.Create(
            applicationName,
            httpContext.TraceIdentifier,
            traceId,
            method,
            path,
            correlationId,
            userId,
            userName,
            clientIp,
            userAgent);
    }
}
