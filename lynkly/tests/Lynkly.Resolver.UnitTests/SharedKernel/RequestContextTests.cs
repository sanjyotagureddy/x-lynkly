using Lynkly.Shared.Kernel.Core;
using Lynkly.Shared.Kernel.Core.Context;

using Microsoft.AspNetCore.Http;

using AppContext = Lynkly.Shared.Kernel.Core.AppContext;

namespace Lynkly.Resolver.UnitTests.SharedKernel;

public sealed class RequestContextTests
{
    [Fact]
    public void FromHttpContext_WhenHeadersExist_PopulatesRequestContext()
    {
    var httpContext = new DefaultHttpContext
    {
      TraceIdentifier = "request-123"
    };
    httpContext.Request.Headers[Constants.Headers.CorrelationId] = "corr-1";
        httpContext.Request.Headers[Constants.Headers.TransactionId] = "txn-1";
        httpContext.Request.Headers[Constants.Headers.TenantId] = "tenant-1";

        var context = RequestContext.FromHttpContext(httpContext, "UrlShortener.Api");

        Assert.Equal("UrlShortener.Api", context.ServiceName);
        Assert.Equal("corr-1", context.CorrelationId);
        Assert.Equal("txn-1", context.TransactionId);
        Assert.Equal("tenant-1", context.TenantId);
        Assert.Equal("request-123", context.RequestId);
    }

    [Fact]
    public void Scope_WhenNested_DisposeRestoresPreviousContext()
    {
        var first = new RequestContext("service-a");
        var second = new RequestContext("service-b");

        using var outer = RequestContextScope.BeginScope(first);
        Assert.Equal("service-a", RequestContextScope.Current.ServiceName);

        using (RequestContextScope.BeginScope(second))
        {
            Assert.Equal("service-b", RequestContextScope.Current.ServiceName);
        }

        Assert.Equal("service-a", RequestContextScope.Current.ServiceName);
    }

    [Fact]
    public void AppContextCurrent_WhenScopeContainsRequestContext_ReturnsMappedAppContext()
    {
        using var scope = RequestContextScope.BeginScope(new RequestContext("service-a")
        {
            CorrelationId = "corr-1",
            TransactionId = "txn-1",
            RequestId = "req-1"
        });

        var appContext = AppContext.Current;

        Assert.Equal("service-a", appContext.ServiceName);
        Assert.Equal("corr-1", appContext.CorrelationId);
        Assert.Equal("txn-1", appContext.TransactionId);
        Assert.Equal("req-1", appContext.RequestId);
    }

    [Fact]
    public void DefaultRequestContextEnricher_WhenCalled_PropagatesHeaders()
    {
        var httpContext = new DefaultHttpContext();
        var requestContext = new RequestContext("UrlShortener.Api")
        {
            CorrelationId = "corr-123",
            TransactionId = "txn-123",
            RequestId = "req-123"
        };

        var enricher = new DefaultRequestContextEnricher();

        enricher.EnrichRequest(httpContext, requestContext);
        enricher.EnrichResponse(httpContext, requestContext);

        Assert.Equal("corr-123", httpContext.Request.Headers[Constants.Headers.CorrelationId]);
        Assert.Equal("txn-123", httpContext.Request.Headers[Constants.Headers.TransactionId]);
        Assert.Equal("req-123", httpContext.Request.Headers[Constants.Headers.RequestId]);

        Assert.Equal("corr-123", httpContext.Response.Headers[Constants.Headers.CorrelationId]);
        Assert.Equal("txn-123", httpContext.Response.Headers[Constants.Headers.TransactionId]);
        Assert.Equal("req-123", httpContext.Response.Headers[Constants.Headers.RequestId]);
    }
}
