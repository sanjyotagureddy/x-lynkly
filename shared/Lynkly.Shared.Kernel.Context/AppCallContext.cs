namespace Lynkly.Shared.Kernel.Context;

/// <summary>
/// Captures ambient per-request metadata that can be propagated across the application pipeline.
/// Intentionally free of ASP.NET Core dependencies so it remains NuGet-extractable.
/// </summary>
public sealed class AppCallContext
{
    private readonly Dictionary<string, string> _items = new(StringComparer.OrdinalIgnoreCase);

    private AppCallContext(
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

    /// <summary>Gets the name of the application that processed the request.</summary>
    public string ApplicationName { get; }

    /// <summary>Gets the unique request identifier assigned by the hosting infrastructure.</summary>
    public string RequestId { get; }

    /// <summary>Gets the distributed trace identifier for the current request.</summary>
    public string TraceId { get; }

    /// <summary>Gets the HTTP method of the request (e.g. <c>GET</c>, <c>POST</c>).</summary>
    public string Method { get; }

    /// <summary>Gets the path portion of the request URL.</summary>
    public string Path { get; }

    /// <summary>Gets or sets the correlation identifier used to correlate requests across services.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Gets or sets the identifier of the authenticated user, if any.</summary>
    public string? UserId { get; set; }

    /// <summary>Gets or sets the display name of the authenticated user, if any.</summary>
    public string? UserName { get; set; }

    /// <summary>Gets or sets the remote IP address of the client, if determinable.</summary>
    public string? ClientIp { get; set; }

    /// <summary>Gets or sets the <c>User-Agent</c> header value sent by the client, if present.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Gets the UTC timestamp at which this context was created.</summary>
    public DateTimeOffset RequestedAtUtc { get; }

    /// <summary>Gets a read-only view of the arbitrary key/value items attached to this context.</summary>
    public IReadOnlyDictionary<string, string> Items => _items;

    /// <summary>
    /// Creates a new <see cref="AppCallContext"/> from the supplied request metadata.
    /// All BCL primitives — no transport framework dependency.
    /// </summary>
    /// <param name="applicationName">The logical application name; must not be <see langword="null"/> or whitespace.</param>
    /// <param name="requestId">The unique request identifier; must not be <see langword="null"/> or whitespace.</param>
    /// <param name="traceId">The distributed trace identifier; must not be <see langword="null"/> or whitespace.</param>
    /// <param name="method">The HTTP method string; must not be <see langword="null"/> or whitespace.</param>
    /// <param name="path">The request path; must not be <see langword="null"/> or whitespace.</param>
    /// <param name="correlationId">Optional correlation identifier for cross-service tracing.</param>
    /// <param name="userId">Optional identifier of the authenticated user.</param>
    /// <param name="userName">Optional display name of the authenticated user.</param>
    /// <param name="clientIp">Optional remote IP address of the client.</param>
    /// <param name="userAgent">Optional <c>User-Agent</c> string.</param>
    /// <returns>A fully initialised <see cref="AppCallContext"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any of the required parameters is <see langword="null"/> or whitespace.
    /// </exception>
    public static AppCallContext Create(
        string applicationName,
        string requestId,
        string traceId,
        string method,
        string path,
        string? correlationId = null,
        string? userId = null,
        string? userName = null,
        string? clientIp = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("Application name is required.", nameof(applicationName));
        }

        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("Request ID is required.", nameof(requestId));
        }

        if (string.IsNullOrWhiteSpace(traceId))
        {
            throw new ArgumentException("Trace ID is required.", nameof(traceId));
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ArgumentException("HTTP method is required.", nameof(method));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Request path is required.", nameof(path));
        }

        return new AppCallContext(
            applicationName,
            requestId,
            traceId,
            method,
            path,
            correlationId,
            userId,
            userName,
            clientIp,
            userAgent);
    }

    /// <summary>
    /// Attaches a named string value to this context.
    /// </summary>
    /// <param name="key">The case-insensitive item key; must not be <see langword="null"/> or whitespace.</param>
    /// <param name="value">The value to store.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or whitespace.</exception>
    public void SetItem(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Item key is required.", nameof(key));
        }

        _items[key] = value;
    }

    /// <summary>
    /// Attempts to retrieve a named value from this context.
    /// </summary>
    /// <param name="key">The case-insensitive item key to look up.</param>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, contains the associated value;
    /// otherwise an empty string.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an item with <paramref name="key"/> was found;
    /// otherwise <see langword="false"/>.
    /// A <see langword="null"/> or whitespace key always returns <see langword="false"/>.
    /// </returns>
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
