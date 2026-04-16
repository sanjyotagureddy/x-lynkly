namespace Lynkly.Shared.Kernel.Core;

/// <summary>
/// Well-known constant values shared across service boundaries.
/// </summary>
public static class Constants
{
    /// <summary>The default API version prefix used in route templates.</summary>
    public const string DefaultApiVersion = "v1";

    /// <summary>Well-known HTTP header names used for tracing, identity, and control.</summary>
    public static class Headers
    {
        // ── Tracing & Observability ───────────────────────────────────────────

        /// <summary>Cross-service correlation identifier header.</summary>
        public const string CorrelationId = "x-correlationid";

        /// <summary>Unique request identifier header.</summary>
        public const string RequestId = "x-request-id";

        /// <summary>Business transaction identifier header.</summary>
        public const string TransactionId = "x-txn-id";

        /// <summary>Distributed trace identifier header.</summary>
        public const string TraceId = "x-traceid";

        /// <summary>Span identifier header.</summary>
        public const string SpanId = "x-spanid";

        /// <summary>Parent span identifier header.</summary>
        public const string ParentSpanId = "x-parent-spanid";

        /// <summary>ISO-8601 UTC timestamp at which the request was initiated.</summary>
        public const string RequestStart = "x-requeststart";

        /// <summary>Originating service name header.</summary>
        public const string ServiceName = "x-servicename";

        // ── Security & Identity ───────────────────────────────────────────────

        /// <summary>Authenticated user identifier header.</summary>
        public const string UserId = "x-userid";

        /// <summary>OAuth2 client identifier header.</summary>
        public const string ClientId = "x-clientid";

        /// <summary>Session identifier header.</summary>
        public const string SessionId = "x-sessionid";

        /// <summary>Tenant identifier header for multi-tenant services.</summary>
        public const string TenantId = "x-tenantid";

        /// <summary>Comma-separated user roles header.</summary>
        public const string Roles = "x-roles";

        /// <summary>OAuth2 scope header.</summary>
        public const string Scope = "x-scope";

        /// <summary>Authentication method used for the request.</summary>
        public const string AuthMethod = "x-auth-method";

        /// <summary>Client device identifier header.</summary>
        public const string DeviceId = "x-deviceid";

        /// <summary>Client application version header.</summary>
        public const string AppVersion = "x-appversion";

        /// <summary>API key header.</summary>
        public const string ApiKey = "x-api-key";

        /// <summary>User token passthrough header.</summary>
        public const string UserToken = "x-user-token";

        // ── Performance & Debugging ───────────────────────────────────────────

        /// <summary>Deployment region header.</summary>
        public const string Region = "x-region";

        /// <summary>Runtime environment header (e.g. production, staging).</summary>
        public const string Environment = "x-environment";

        /// <summary>Requested API version header.</summary>
        public const string ApiVersion = "x-apiversion";

        /// <summary>Feature flag override header.</summary>
        public const string FeatureFlag = "x-feature-flag";

        /// <summary>Debug mode enable header.</summary>
        public const string DebugMode = "x-debug-mode";

        // ── Standard Request Headers ──────────────────────────────────────────

        /// <summary>User-Agent header.</summary>
        public const string UserAgent = "user-agent";

        /// <summary>Authorization header.</summary>
        public const string Authorization = "authorization";

        /// <summary>Content-Type header.</summary>
        public const string ContentType = "content-type";

        /// <summary>Accept header.</summary>
        public const string Accept = "accept";

        /// <summary>Accept-Encoding header.</summary>
        public const string AcceptEncoding = "accept-encoding";

        /// <summary>Accept-Language header.</summary>
        public const string AcceptLanguage = "accept-language";

        /// <summary>Host header.</summary>
        public const string Host = "host";

        /// <summary>Referer header.</summary>
        public const string Referer = "referer";

        /// <summary>Origin header.</summary>
        public const string Origin = "origin";

        /// <summary>Connection header.</summary>
        public const string Connection = "connection";

        /// <summary>Cache-Control header.</summary>
        public const string CacheControl = "cache-control";

        /// <summary>Pragma header.</summary>
        public const string Pragma = "pragma";

        /// <summary>X-Forwarded-For header.</summary>
        public const string XForwardedFor = "x-forwarded-for";

        /// <summary>X-Forwarded-Proto header.</summary>
        public const string XForwardedProto = "x-forwarded-proto";

        /// <summary>X-Forwarded-Port header.</summary>
        public const string XForwardedPort = "x-forwarded-port";

        /// <summary>X-Real-IP header.</summary>
        public const string XRealIp = "x-real-ip";

        /// <summary>If-Modified-Since header.</summary>
        public const string IfModifiedSince = "if-modified-since";

        /// <summary>If-None-Match header.</summary>
        public const string IfNoneMatch = "if-none-match";

        /// <summary>Range header.</summary>
        public const string Range = "range";

        // ── Standard Response Headers ─────────────────────────────────────────

        /// <summary>Set-Cookie header.</summary>
        public const string SetCookie = "set-cookie";

        /// <summary>Expires header.</summary>
        public const string Expires = "expires";

        /// <summary>Last-Modified header.</summary>
        public const string LastModified = "last-modified";

        /// <summary>ETag header.</summary>
        public const string ETag = "etag";

        /// <summary>X-Powered-By header.</summary>
        public const string XPoweredBy = "x-powered-by";

        // ── CORS Headers ──────────────────────────────────────────────────────

        /// <summary>Access-Control-Allow-Origin header.</summary>
        public const string AccessControlAllowOrigin = "access-control-allow-origin";

        /// <summary>Access-Control-Allow-Credentials header.</summary>
        public const string AccessControlAllowCredentials = "access-control-allow-credentials";

        /// <summary>Access-Control-Allow-Methods header.</summary>
        public const string AccessControlAllowMethods = "access-control-allow-methods";

        /// <summary>Access-Control-Allow-Headers header.</summary>
        public const string AccessControlAllowHeaders = "access-control-allow-headers";

        // ── Security Response Headers ─────────────────────────────────────────

        /// <summary>X-Frame-Options header.</summary>
        public const string XFrameOptions = "x-frame-options";

        /// <summary>X-XSS-Protection header.</summary>
        public const string XXssProtection = "x-xss-protection";

        /// <summary>X-Content-Type-Options header.</summary>
        public const string XContentTypeOptions = "x-content-type-options";

        /// <summary>Strict-Transport-Security header.</summary>
        public const string StrictTransportSecurity = "strict-transport-security";

        /// <summary>Referrer-Policy header.</summary>
        public const string ReferrerPolicy = "referrer-policy";

        /// <summary>Feature-Policy header.</summary>
        public const string FeaturePolicy = "feature-policy";

        /// <summary>Content-Security-Policy header.</summary>
        public const string ContentSecurityPolicy = "content-security-policy";
    }
}
