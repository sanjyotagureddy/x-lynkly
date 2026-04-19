namespace Lynkly.Shared.Kernel.Core.Exceptions;

public static class ErrorResponseFactory
{
    public static ErrorResponse Create(Exception exception, string? traceId = null, DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var now = timestampUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        if (exception is BaseAppException appException)
        {
            return new ErrorResponse(
                appException.Code,
                appException.Message,
                appException.StatusCode,
                appException.Errors,
                traceId,
                now);
        }

        return new ErrorResponse(
            ExceptionCodes.Unexpected,
            "An unexpected error occurred.",
            StatusCodes.InternalServerError,
            [],
            traceId,
            now);
    }

    private static class StatusCodes
    {
        public const int InternalServerError = 500;
    }
}
