namespace Lynkly.Shared.Kernel.Exceptions;

public abstract class BaseAppException : Exception
{
    protected BaseAppException(
        string code,
        string message,
        int statusCode,
        IEnumerable<ErrorDetail>? errors = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        if (statusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Status code must be a valid HTTP status code.");
        }

        Code = code;
        StatusCode = statusCode;
        Errors = errors?.ToArray() ?? [];
    }

    public string Code { get; }

    public int StatusCode { get; }

    public IReadOnlyList<ErrorDetail> Errors { get; }
}
