using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Lynkly.Shared.Kernel.Context;

public sealed class AppContext
{
    private readonly Dictionary<string, string> _items = new(StringComparer.OrdinalIgnoreCase);

    private AppContext(
        string applicationName,
        string requestId,
        string traceId,
        string method,
        string path,
        string? correlationId,
        string? userId,
        string? userName,
        string? clientIp,
        string? userAgent)
    {
        ApplicationName = applicationName;
        RequestId = requestId;
        TraceId = traceId;
        Method = method;
        Path = path;
        CorrelationId = correlationId;
        UserId = userId;
        UserName = userName;
        ClientIp = clientIp;
        UserAgent = userAgent;
        RequestedAtUtc = DateTimeOffset.UtcNow;
    }

    public string ApplicationName { get; }

    public string RequestId { get; }

    public string TraceId { get; }

    public string Method { get; }

    public string Path { get; }

    public string? CorrelationId { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset RequestedAtUtc { get; }

    public IReadOnlyDictionary<string, string> Items => _items;

    public static AppContext FromHttpContext(HttpContext httpContext, string applicationName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("Application name is required.", nameof(applicationName));
        }

        var request = httpContext.Request;
        var correlationId = request.Headers.TryGetValue("X-Correlation-Id", out var correlationValue)
            ? correlationValue.ToString()
            : null;
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var method = string.IsNullOrWhiteSpace(request.Method) ? HttpMethods.Get : request.Method;
        var path = request.Path.HasValue ? request.Path.Value! : "/";
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = httpContext.User.Identity?.Name;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = request.Headers.UserAgent.ToString();

        return new AppContext(
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

    public void SetItem(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Item key is required.", nameof(key));
        }

        _items[key] = value;
    }

    public bool TryGetItem(string key, out string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            value = string.Empty;
            return false;
        }

        return _items.TryGetValue(key, out value!);
    }
}
