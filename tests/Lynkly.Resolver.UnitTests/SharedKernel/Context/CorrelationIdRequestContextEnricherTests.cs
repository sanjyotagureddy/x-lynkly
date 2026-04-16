using Lynkly.Resolver.API.Middlewares;
using Lynkly.Shared.Kernel.Context;
using Lynkly.Shared.Kernel.Core;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Context;

public sealed class CorrelationIdRequestContextEnricherTests
{
    private readonly CorrelationIdRequestContextEnricher _enricher = new();

    // ── EnrichRequest: guard clauses ─────────────────────────────────────────

    [Fact]
    public void EnrichRequest_WithNullHttpContext_ThrowsArgumentNullException()
    {
        var ctx = MakeAppCallContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichRequest(null!, ctx));
    }

    [Fact]
    public void EnrichRequest_WithNullAppCallContext_ThrowsArgumentNullException()
    {
        var httpContext = new DefaultHttpContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichRequest(httpContext, null!));
    }

    // ── EnrichRequest: correlation logic ────────────────────────────────────

    [Fact]
    public void EnrichRequest_WhenCorrelationIdAlreadySet_DoesNotOverride()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppCallContext(correlationId: "existing-id");

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.Equal("existing-id", ctx.CorrelationId);
    }

    [Fact]
    public void EnrichRequest_WhenHeaderPresent_UsesHeaderValue()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[Constants.Headers.CorrelationId] = "header-corr-id";
        var ctx = MakeAppCallContext();

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.Equal("header-corr-id", ctx.CorrelationId);
    }

    [Fact]
    public void EnrichRequest_WhenHeaderIsWhitespace_GeneratesNewId()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[Constants.Headers.CorrelationId] = "   ";
        var ctx = MakeAppCallContext();

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.NotNull(ctx.CorrelationId);
        Assert.NotEmpty(ctx.CorrelationId);
        Assert.NotEqual("   ", ctx.CorrelationId);
    }

    [Fact]
    public void EnrichRequest_WhenNoHeaderAndNoExistingId_GeneratesNewGuid()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppCallContext();

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.NotNull(ctx.CorrelationId);
        Assert.True(Guid.TryParseExact(ctx.CorrelationId, "N", out _),
            "Expected a 32-character lowercase hex GUID");
    }

    // ── EnrichResponse: guard clauses ────────────────────────────────────────

    [Fact]
    public void EnrichResponse_WithNullHttpContext_ThrowsArgumentNullException()
    {
        var ctx = MakeAppCallContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichResponse(null!, ctx));
    }

    [Fact]
    public void EnrichResponse_WithNullAppCallContext_ThrowsArgumentNullException()
    {
        var httpContext = new DefaultHttpContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichResponse(httpContext, null!));
    }

    // ── EnrichResponse: header writing ──────────────────────────────────────

    [Fact]
    public void EnrichResponse_WhenCorrelationIdIsSet_WritesResponseHeader()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppCallContext(correlationId: "resp-corr");

        _enricher.EnrichResponse(httpContext, ctx);

        Assert.Equal("resp-corr", httpContext.Response.Headers[Constants.Headers.CorrelationId].ToString());
    }

    [Fact]
    public void EnrichResponse_WhenCorrelationIdIsNull_DoesNotWriteHeader()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppCallContext();

        _enricher.EnrichResponse(httpContext, ctx);

        Assert.False(httpContext.Response.Headers.ContainsKey(Constants.Headers.CorrelationId));
    }

    [Fact]
    public void EnrichResponse_WhenCorrelationIdIsWhitespace_DoesNotWriteHeader()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppCallContext();
        ctx.CorrelationId = "  ";

        _enricher.EnrichResponse(httpContext, ctx);

        Assert.False(httpContext.Response.Headers.ContainsKey(Constants.Headers.CorrelationId));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AppCallContext MakeAppCallContext(string? correlationId = null)
    {
        var ctx = AppCallContext.Create("app", "req-1", "trace-1", "GET", "/");
        ctx.CorrelationId = correlationId;
        return ctx;
    }
}
