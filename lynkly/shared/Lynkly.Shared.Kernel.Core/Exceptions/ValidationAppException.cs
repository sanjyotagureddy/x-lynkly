namespace Lynkly.Shared.Kernel.Core.Exceptions;

public sealed class ValidationAppException : BaseAppException
{
    public ValidationAppException(IEnumerable<ErrorDetail> errors, string? message = null)
        : base(
            ExceptionCodes.ValidationFailed,
            message ?? DefaultMessage,
            StatusCodes.BadRequest,
            errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors)))
    {
    }

    private const string DefaultMessage = "Validation failed for the request.";

    private static class StatusCodes
    {
        public const int BadRequest = 400;
    }
}
