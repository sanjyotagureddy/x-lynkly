using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Lynkly.Shared.Kernel.Context;

public sealed class AppContext
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    public required string ApplicationName { get; init; }

    public required string CorrelationId { get; set; }

    public required string TraceId { get; init; }

    public required string RequestId { get; init; }

    public required string HttpMethod { get; init; }

    public required string Path { get; init; }

    public DateTimeOffset RequestedAtUtc { get; init; }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    public static AppContext FromHttpContext(HttpContext httpContext, string applicationName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("Application name cannot be null or whitespace.", nameof(applicationName));
        }

        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var correlationId = GetFirstRequestHeaderValue(httpContext, CorrelationIdHeaderName);

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = traceId;
        }

        return new AppContext
        {
            ApplicationName = applicationName,
            CorrelationId = correlationId,
            TraceId = traceId,
            RequestId = httpContext.TraceIdentifier,
            HttpMethod = httpContext.Request.Method,
            Path = httpContext.Request.Path.ToString(),
            RequestedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static string? GetFirstRequestHeaderValue(HttpContext httpContext, string headerName)
    {
        if (!httpContext.Request.Headers.TryGetValue(headerName, out var values) || values.Count == 0)
        {
            return null;
        }

        var headerValue = values[0];
        return string.IsNullOrWhiteSpace(headerValue) ? null : headerValue;
    }
}
