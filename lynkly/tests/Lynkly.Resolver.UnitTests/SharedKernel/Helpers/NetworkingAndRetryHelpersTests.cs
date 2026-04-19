using System.Net;
using System.Text;

using Lynkly.Shared.Kernel.Core.Exceptions;
using Lynkly.Shared.Kernel.Core.Helpers;
using Lynkly.Shared.Kernel.Core.Helpers.Networking;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class NetworkingAndRetryHelpersTests
{
    [Fact]
    public async Task HttpRequestHelper_Should_BuildSendAndHandleResponse()
    {
        var query = HttpRequestHelper.BuildQueryString(new Dictionary<string, string?>
        {
            ["key"] = "value",
            ["with space"] = "a b"
        });

        Assert.StartsWith("?", query);
        Assert.Contains("key=value", query);
        Assert.Contains("with%20space=a%20b", query);

        using var client = new HttpClient(new TestMessageHandler(HttpStatusCode.Accepted, "ok"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test");

        HttpRequestHelper.AddHeaders(request, new Dictionary<string, string?>
        {
            ["x-test"] = "one",
            [""] = "ignore"
        });

        var result = await HttpRequestHelper.SendAndReadSafeAsync(client, request);

        Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal("ok", result.Content);
        Assert.Equal(string.Empty, HttpRequestHelper.BuildQueryString(null));
        Assert.Equal(string.Empty, HttpRequestHelper.BuildQueryString(new Dictionary<string, string?> { [" "] = "x" }));

        Assert.Throws<ArgumentNullException>(() => HttpRequestHelper.AddHeaders(null!, new Dictionary<string, string?>()));
        Assert.Throws<ArgumentNullException>(() => HttpRequestHelper.AddHeaders(request, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => HttpRequestHelper.SendAsync(null!, request));
        await Assert.ThrowsAsync<ArgumentNullException>(() => HttpRequestHelper.SendAsync(client, null!));
    }

    [Fact]
    public async Task RetryHelper_Should_RetryAndHonorFilter()
    {
        var attempts = 0;

        var result = await RetryHelper.ExecuteAsync<int>(
            _ =>
            {
                attempts++;
                if (attempts < 3)
                {
                    throw new InvalidOperationException("retry");
                }

                return Task.FromResult(42);
            },
            new RetryPolicyOptions
            {
                RetryCount = 3,
                InitialDelay = TimeSpan.FromMilliseconds(1),
                DelayStrategy = RetryDelayStrategy.Fixed,
                ExceptionFilter = ex => ex is InvalidOperationException
            });

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);

        await Assert.ThrowsAsync<ArgumentNullException>(() => RetryHelper.ExecuteAsync<int>(null!));
        await Assert.ThrowsAsync<SharedKernelException>(() => RetryHelper.ExecuteAsync<int>(
            _ => Task.FromResult(1),
            new RetryPolicyOptions { RetryCount = -1 }));
        await Assert.ThrowsAsync<SharedKernelException>(() => RetryHelper.ExecuteAsync<int>(
            _ => Task.FromResult(1),
            new RetryPolicyOptions { InitialDelay = TimeSpan.FromMilliseconds(-1) }));

        var noRetryAttempts = 0;
        await Assert.ThrowsAsync<ApplicationException>(() => RetryHelper.ExecuteAsync<int>(
            _ =>
            {
                noRetryAttempts++;
                throw new ApplicationException();
            },
            new RetryPolicyOptions
            {
                RetryCount = 5,
                InitialDelay = TimeSpan.Zero,
                ExceptionFilter = _ => false
            }));

        Assert.Equal(1, noRetryAttempts);

        var nonSuccess = new HttpResponseResult(HttpStatusCode.BadRequest, "bad");
        Assert.False(nonSuccess.IsSuccessStatusCode);
    }

    [Fact]
    public async Task RetryHelper_ExecuteAsync_NonGeneric_Should_Work()
    {
        var attempts = 0;
        await RetryHelper.ExecuteAsync(
            _ =>
            {
                attempts++;
                return attempts < 2 ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
            },
            new RetryPolicyOptions
            {
                RetryCount = 2,
                DelayStrategy = RetryDelayStrategy.ExponentialBackoff,
                InitialDelay = TimeSpan.FromMilliseconds(1)
            });

        Assert.Equal(2, attempts);

        await Assert.ThrowsAsync<ArgumentNullException>(() => RetryHelper.ExecuteAsync(null!));
    }

    [Fact]
    public async Task RetryHelper_Should_FallbackToDefaultDelay_WhenStrategyUnknown()
    {
        var attempts = 0;

        await RetryHelper.ExecuteAsync(
            _ =>
            {
                attempts++;
                return attempts < 2 ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
            },
            new RetryPolicyOptions
            {
                RetryCount = 1,
                InitialDelay = TimeSpan.Zero,
                DelayStrategy = (RetryDelayStrategy)999
            });

        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task RetryHelper_ExponentialBackoff_Should_HandleZeroDelay()
    {
        var attempts = 0;

        await RetryHelper.ExecuteAsync(
            _ =>
            {
                attempts++;
                return attempts < 2 ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
            },
            new RetryPolicyOptions
            {
                RetryCount = 1,
                InitialDelay = TimeSpan.Zero,
                DelayStrategy = RetryDelayStrategy.ExponentialBackoff
            });

        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task RetryHelper_ExponentialBackoff_Should_NotOverflowLargeDelays()
    {
        var attempts = 0;
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RetryHelper.ExecuteAsync(
            _ =>
            {
                attempts++;
                cts.Cancel();
                return Task.FromException(new InvalidOperationException("force retry"));
            },
            new RetryPolicyOptions
            {
                RetryCount = 1,
                InitialDelay = TimeSpan.MaxValue,
                DelayStrategy = RetryDelayStrategy.ExponentialBackoff
            },
            cts.Token));

        Assert.Equal(1, attempts);
    }

    private sealed class TestMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/plain")
            });
        }
    }
}
