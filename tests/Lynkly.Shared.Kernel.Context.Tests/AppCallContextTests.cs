using Lynkly.Shared.Kernel.Context;

namespace Lynkly.Shared.Kernel.Context.Tests;

public sealed class AppCallContextTests
{
    // ── Create: happy path ──────────────────────────────────────────────────

    [Fact]
    public void Create_WithAllParameters_SetsAllProperties()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var ctx = AppCallContext.Create(
            applicationName: "test-app",
            requestId: "req-1",
            traceId: "trace-1",
            method: "POST",
            path: "/links/abc",
            correlationId: "corr-1",
            userId: "user-1",
            userName: "alice",
            clientIp: "127.0.0.1",
            userAgent: "TestAgent/1.0");

        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.Equal("test-app", ctx.ApplicationName);
        Assert.Equal("req-1", ctx.RequestId);
        Assert.Equal("trace-1", ctx.TraceId);
        Assert.Equal("POST", ctx.Method);
        Assert.Equal("/links/abc", ctx.Path);
        Assert.Equal("corr-1", ctx.CorrelationId);
        Assert.Equal("user-1", ctx.UserId);
        Assert.Equal("alice", ctx.UserName);
        Assert.Equal("127.0.0.1", ctx.ClientIp);
        Assert.Equal("TestAgent/1.0", ctx.UserAgent);
        Assert.InRange(ctx.RequestedAtUtc, before, after);
    }

    [Fact]
    public void Create_WithOnlyRequiredParameters_SetsNullablePropertiesToNull()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        Assert.Null(ctx.CorrelationId);
        Assert.Null(ctx.UserId);
        Assert.Null(ctx.UserName);
        Assert.Null(ctx.ClientIp);
        Assert.Null(ctx.UserAgent);
    }

    [Fact]
    public void Create_WithOnlyRequiredParameters_ItemsIsEmpty()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        Assert.Empty(ctx.Items);
    }

    // ── Create: applicationName guard ───────────────────────────────────────

    [Fact]
    public void Create_WithNullApplicationName_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create(null!, "req", "trace", "GET", "/"));

        Assert.Equal("applicationName", ex.ParamName);
    }

    [Fact]
    public void Create_WithEmptyApplicationName_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create(string.Empty, "req", "trace", "GET", "/"));

        Assert.Equal("applicationName", ex.ParamName);
    }

    [Fact]
    public void Create_WithWhitespaceApplicationName_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("   ", "req", "trace", "GET", "/"));

        Assert.Equal("applicationName", ex.ParamName);
    }

    // ── Create: requestId guard ──────────────────────────────────────────────

    [Fact]
    public void Create_WithNullRequestId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", null!, "trace", "GET", "/"));

        Assert.Equal("requestId", ex.ParamName);
    }

    [Fact]
    public void Create_WithEmptyRequestId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", string.Empty, "trace", "GET", "/"));

        Assert.Equal("requestId", ex.ParamName);
    }

    [Fact]
    public void Create_WithWhitespaceRequestId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "  ", "trace", "GET", "/"));

        Assert.Equal("requestId", ex.ParamName);
    }

    // ── Create: traceId guard ────────────────────────────────────────────────

    [Fact]
    public void Create_WithNullTraceId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", null!, "GET", "/"));

        Assert.Equal("traceId", ex.ParamName);
    }

    [Fact]
    public void Create_WithEmptyTraceId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", string.Empty, "GET", "/"));

        Assert.Equal("traceId", ex.ParamName);
    }

    [Fact]
    public void Create_WithWhitespaceTraceId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "\t", "GET", "/"));

        Assert.Equal("traceId", ex.ParamName);
    }

    // ── Create: method guard ─────────────────────────────────────────────────

    [Fact]
    public void Create_WithNullMethod_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "trace", null!, "/"));

        Assert.Equal("method", ex.ParamName);
    }

    [Fact]
    public void Create_WithEmptyMethod_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "trace", string.Empty, "/"));

        Assert.Equal("method", ex.ParamName);
    }

    [Fact]
    public void Create_WithWhitespaceMethod_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "trace", " ", "/"));

        Assert.Equal("method", ex.ParamName);
    }

    // ── Create: path guard ───────────────────────────────────────────────────

    [Fact]
    public void Create_WithNullPath_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "trace", "GET", null!));

        Assert.Equal("path", ex.ParamName);
    }

    [Fact]
    public void Create_WithEmptyPath_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "trace", "GET", string.Empty));

        Assert.Equal("path", ex.ParamName);
    }

    [Fact]
    public void Create_WithWhitespacePath_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AppCallContext.Create("app", "req", "trace", "GET", "  "));

        Assert.Equal("path", ex.ParamName);
    }

    // ── Mutable properties ───────────────────────────────────────────────────

    [Fact]
    public void CorrelationId_CanBeSetAfterCreation()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.CorrelationId = "new-corr";
        Assert.Equal("new-corr", ctx.CorrelationId);
    }

    [Fact]
    public void UserId_CanBeSetAfterCreation()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.UserId = "u-99";
        Assert.Equal("u-99", ctx.UserId);
    }

    [Fact]
    public void UserName_CanBeSetAfterCreation()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.UserName = "bob";
        Assert.Equal("bob", ctx.UserName);
    }

    [Fact]
    public void ClientIp_CanBeSetAfterCreation()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.ClientIp = "10.0.0.1";
        Assert.Equal("10.0.0.1", ctx.ClientIp);
    }

    [Fact]
    public void UserAgent_CanBeSetAfterCreation()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.UserAgent = "curl/7.0";
        Assert.Equal("curl/7.0", ctx.UserAgent);
    }

    // ── SetItem ──────────────────────────────────────────────────────────────

    [Fact]
    public void SetItem_WithValidKey_StoresValue()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.SetItem("tenant", "acme");
        Assert.True(ctx.Items.ContainsKey("tenant"));
        Assert.Equal("acme", ctx.Items["tenant"]);
    }

    [Fact]
    public void SetItem_IsCaseInsensitive()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.SetItem("Tenant", "acme");
        Assert.True(ctx.Items.ContainsKey("tenant"));
        Assert.True(ctx.Items.ContainsKey("TENANT"));
    }

    [Fact]
    public void SetItem_Overwrites_ExistingValue()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.SetItem("key", "first");
        ctx.SetItem("key", "second");
        Assert.Equal("second", ctx.Items["key"]);
    }

    [Fact]
    public void SetItem_WithNullKey_ThrowsArgumentException()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var ex = Assert.Throws<ArgumentException>(() => ctx.SetItem(null!, "value"));
        Assert.Equal("key", ex.ParamName);
    }

    [Fact]
    public void SetItem_WithEmptyKey_ThrowsArgumentException()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var ex = Assert.Throws<ArgumentException>(() => ctx.SetItem(string.Empty, "value"));
        Assert.Equal("key", ex.ParamName);
    }

    [Fact]
    public void SetItem_WithWhitespaceKey_ThrowsArgumentException()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var ex = Assert.Throws<ArgumentException>(() => ctx.SetItem("  ", "value"));
        Assert.Equal("key", ex.ParamName);
    }

    // ── TryGetItem ───────────────────────────────────────────────────────────

    [Fact]
    public void TryGetItem_WithExistingKey_ReturnsTrueAndValue()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.SetItem("k", "v");

        var found = ctx.TryGetItem("k", out var value);

        Assert.True(found);
        Assert.Equal("v", value);
    }

    [Fact]
    public void TryGetItem_WithMissingKey_ReturnsFalseAndNull()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var found = ctx.TryGetItem("missing", out var value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetItem_WithNullKey_ReturnsFalseAndEmptyString()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var found = ctx.TryGetItem(null!, out var value);

        Assert.False(found);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TryGetItem_WithEmptyKey_ReturnsFalseAndEmptyString()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var found = ctx.TryGetItem(string.Empty, out var value);

        Assert.False(found);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TryGetItem_WithWhitespaceKey_ReturnsFalseAndEmptyString()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");

        var found = ctx.TryGetItem("  ", out var value);

        Assert.False(found);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TryGetItem_IsCaseInsensitive()
    {
        var ctx = AppCallContext.Create("app", "req", "trace", "GET", "/");
        ctx.SetItem("MyKey", "myValue");

        var found = ctx.TryGetItem("mykey", out var value);

        Assert.True(found);
        Assert.Equal("myValue", value);
    }
}
