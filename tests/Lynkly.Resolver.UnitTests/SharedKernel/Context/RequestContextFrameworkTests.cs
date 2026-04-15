using Lynkly.Resolver.API.Middlewares;
using Lynkly.Shared.Kernel.Context;
using RequestAppContext = Lynkly.Shared.Kernel.Context.AppContext;
using Lynkly.Shared.Kernel.Context.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Context;

public sealed class RequestContextFrameworkTests
{
    [Fact]
    public void FromHttpContext_Should_Use_Existing_CorrelationId_Header_When_Present()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-id-123";
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/v1/links";
        httpContext.Request.Headers[RequestAppContext.CorrelationIdHeaderName] = "corr-001";

        var appContext = RequestAppContext.FromHttpContext(httpContext, "Lynkly.Resolver.API");

        Assert.Equal("Lynkly.Resolver.API", appContext.ApplicationName);
        Assert.Equal("corr-001", appContext.CorrelationId);
        Assert.Equal("trace-id-123", appContext.TraceId);
        Assert.Equal("trace-id-123", appContext.RequestId);
        Assert.Equal("GET", appContext.HttpMethod);
        Assert.Equal("/api/v1/links", appContext.Path);
    }

    [Fact]
    public void FromHttpContext_Should_Fallback_To_TraceId_When_CorrelationId_Is_Missing()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-id-xyz";

        var appContext = RequestAppContext.FromHttpContext(httpContext, "Lynkly.Resolver.API");

        Assert.Equal("trace-id-xyz", appContext.CorrelationId);
    }

    [Fact]
    public void RequestContextScope_Should_Set_And_Restore_Current_Context()
    {
        var originalContext = new RequestAppContext
        {
            ApplicationName = "Original",
            CorrelationId = "orig-corr",
            TraceId = "orig-trace",
            RequestId = "orig-req",
            HttpMethod = "GET",
            Path = "/",
            RequestedAtUtc = DateTimeOffset.UtcNow
        };

        var nestedContext = new RequestAppContext
        {
            ApplicationName = "Nested",
            CorrelationId = "nested-corr",
            TraceId = "nested-trace",
            RequestId = "nested-req",
            HttpMethod = "POST",
            Path = "/nested",
            RequestedAtUtc = DateTimeOffset.UtcNow
        };

        using (RequestContextScope.BeginScope(originalContext))
        {
            Assert.Same(originalContext, RequestContextScope.Current);

            using (RequestContextScope.BeginScope(nestedContext))
            {
                Assert.Same(nestedContext, RequestContextScope.Current);
            }

            Assert.Same(originalContext, RequestContextScope.Current);
        }

        Assert.Null(RequestContextScope.Current);
    }

    [Fact]
    public async Task Middleware_Should_Run_Enrichers_And_Manage_Scope()
    {
        var hostEnvironment = new TestHostEnvironment
        {
            ApplicationName = "Lynkly.Resolver.API"
        };

        var enricher = new RecordingEnricher();
        RequestAppContext? capturedDuringNext = null;

        RequestDelegate next = _ =>
        {
            capturedDuringNext = RequestContextScope.Current;
            return Task.CompletedTask;
        };

        var middleware = new RequestContextMiddleware(next, hostEnvironment, [enricher]);

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-id-next";

        await middleware.InvokeAsync(httpContext);

        Assert.Equal(1, enricher.EnrichRequestCallCount);
        Assert.Equal(1, enricher.EnrichResponseCallCount);
        Assert.NotNull(capturedDuringNext);
        Assert.Equal("trace-id-next", capturedDuringNext!.CorrelationId);
        Assert.Null(RequestContextScope.Current);
        Assert.Equal("trace-id-next", httpContext.Response.Headers[RequestAppContext.CorrelationIdHeaderName].ToString());
    }

    private sealed class RecordingEnricher : IRequestContextEnricher
    {
        public int EnrichRequestCallCount { get; private set; }

        public int EnrichResponseCallCount { get; private set; }

        public void EnrichRequest(HttpContext httpContext, RequestAppContext appContext)
        {
            EnrichRequestCallCount++;
            httpContext.Request.Headers[RequestAppContext.CorrelationIdHeaderName] = appContext.CorrelationId;
        }

        public void EnrichResponse(HttpContext httpContext, RequestAppContext appContext)
        {
            EnrichResponseCallCount++;
            httpContext.Response.Headers[RequestAppContext.CorrelationIdHeaderName] = appContext.CorrelationId;
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "Lynkly";

        public string ContentRootPath { get; set; } = System.AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
