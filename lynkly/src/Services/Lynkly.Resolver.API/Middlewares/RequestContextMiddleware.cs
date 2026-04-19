using Lynkly.Shared.Kernel.Core.Context;
using Lynkly.Shared.Kernel.Logging.Abstractions;

namespace Lynkly.Resolver.API.Middlewares;

public sealed class RequestContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IEnumerable<IRequestContextEnricher> enrichers,
    IStructuredLogger<RequestContextMiddleware> logger)
{
    private readonly IStructuredLogger<RequestContextMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var appContext = Shared.Kernel.Core.AppContext.FromHttpContext(httpContext, environment.ApplicationName);

        foreach (var enricher in enrichers)
        {
            enricher.EnrichRequest(httpContext, appContext);
        }

        using (RequestContextScope.BeginScope(appContext))
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["RequestId"] = appContext.RequestId,
                ["CorrelationId"] = appContext.CorrelationId,
                ["TransactionId"] = appContext.TransactionId,
                ["TraceId"] = appContext.TraceId,
                ["TenantId"] = appContext.TenantId
            });

            _logger.LogInformation(
                "Request started {Method} {Path} RequestId {RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path.ToString(),
                appContext.RequestId);

            httpContext.Response.OnStarting(() =>
            {
                foreach (var enricher in enrichers)
                {
                    enricher.EnrichResponse(httpContext, appContext);
                }

                return Task.CompletedTask;
            });

            try
            {
                await next(httpContext);
            }
            finally
            {
                _logger.LogInformation(
                    "Request completed {Method} {Path} StatusCode {StatusCode} RequestId {RequestId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path.ToString(),
                    httpContext.Response.StatusCode,
                    appContext.RequestId);
            }
        }
    }

}
