using System.Net;

namespace Lynkly.Shared.Kernel.Core.Helpers.Networking;

/// <summary>
/// Represents a safe HTTP response payload.
/// </summary>
public sealed record HttpResponseResult(HttpStatusCode StatusCode, string Content)
{
    /// <summary>
    /// Indicates whether the status code is successful.
    /// </summary>
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
}

/// <summary>
/// Provides HTTP request helper operations.
/// </summary>
public static class HttpRequestHelper
{
    /// <summary>
    /// Builds a query string from name/value parameters.
    /// </summary>
    public static string BuildQueryString(IReadOnlyDictionary<string, string?>? queryParameters)
    {
        if (queryParameters is null || queryParameters.Count == 0)
        {
            return string.Empty;
        }

        var pairs = queryParameters
            .Where(static parameter => !string.IsNullOrWhiteSpace(parameter.Key))
            .Select(static parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value ?? string.Empty)}")
            .ToArray();

        return pairs.Length == 0 ? string.Empty : $"?{string.Join("&", pairs)}";
    }

    /// <summary>
    /// Adds request headers, replacing existing values.
    /// </summary>
    public static void AddHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?> headers)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(headers);

        foreach (var (key, value) in headers)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            request.Headers.Remove(key);
            request.Headers.TryAddWithoutValidation(key, value ?? string.Empty);
        }
    }

    /// <summary>
    /// Sends an HTTP request and returns the response.
    /// </summary>
    public static Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(request);
        return httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request and safely reads response content.
    /// </summary>
    public static async Task<HttpResponseResult> SendAndReadSafeAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(httpClient, request, cancellationToken).ConfigureAwait(false);
        var content = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return new HttpResponseResult(response.StatusCode, content);
    }
}
