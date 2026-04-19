using Lynkly.Shared.Kernel.Core.Exceptions;
using Lynkly.Shared.Kernel.Core.Exceptions.UrlShortener;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Exceptions;

public sealed class ExceptionFrameworkTests
{
    [Fact]
    public void BaseException_Should_Expose_Metadata_And_Errors()
    {
        var exception = new TestAppException(
            "test.code",
            "test message",
            422,
            [new ErrorDetail("field", "message", "detail.code")]);

        Assert.Equal("test.code", exception.Code);
        Assert.Equal("test message", exception.Message);
        Assert.Equal(422, exception.StatusCode);
        Assert.Single(exception.Errors);
        Assert.Equal("field", exception.Errors[0].Field);
    }

    [Fact]
    public void ErrorResponseFactory_Should_Map_BaseAppException()
    {
        var timestamp = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var exception = new TestAppException(
            "test.code",
            "test message",
            400,
            [new ErrorDetail("field", "message")]);

        var response = ErrorResponseFactory.Create(exception, "trace-123", timestamp);

        Assert.Equal("test.code", response.Code);
        Assert.Equal("test message", response.Message);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("trace-123", response.TraceId);
        Assert.Equal(timestamp, response.TimestampUtc);
        Assert.Single(response.Errors);
    }

    [Fact]
    public void ErrorResponseFactory_Should_Map_Unexpected_Exception()
    {
        var response = ErrorResponseFactory.Create(new InvalidOperationException("boom"), "trace-xyz");

        Assert.Equal(ExceptionCodes.Unexpected, response.Code);
        Assert.Equal("An unexpected error occurred.", response.Message);
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("trace-xyz", response.TraceId);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void UrlShortenerExceptions_Should_Have_Expected_Codes_And_Statuses()
    {
        var notFound = new LinkNotFoundException("abc123");
        var duplicate = new AliasAlreadyExistsException("promo");
        var invalidUrl = new InvalidDestinationUrlException("ftp://invalid");
        var expired = new LinkExpiredException("spring-sale");

        Assert.Equal(ExceptionCodes.UrlShortener.LinkNotFound, notFound.Code);
        Assert.Equal(404, notFound.StatusCode);

        Assert.Equal(ExceptionCodes.UrlShortener.AliasAlreadyExists, duplicate.Code);
        Assert.Equal(409, duplicate.StatusCode);

        Assert.Equal(ExceptionCodes.UrlShortener.InvalidDestinationUrl, invalidUrl.Code);
        Assert.Equal(400, invalidUrl.StatusCode);

        Assert.Equal(ExceptionCodes.UrlShortener.LinkExpired, expired.Code);
        Assert.Equal(410, expired.StatusCode);
    }

    private sealed class TestAppException : BaseAppException
    {
        public TestAppException(
            string code,
            string message,
            int statusCode,
            IEnumerable<ErrorDetail>? errors = null,
            Exception? innerException = null)
            : base(code, message, statusCode, errors, innerException)
        {
        }
    }
}
