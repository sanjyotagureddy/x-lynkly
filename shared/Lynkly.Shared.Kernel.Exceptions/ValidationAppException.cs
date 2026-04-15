using FluentValidation.Results;

namespace Lynkly.Shared.Kernel.Exceptions;

public sealed class ValidationAppException : BaseAppException
{
    public ValidationAppException(IEnumerable<ValidationFailure> failures, string? message = null)
        : base(
            ExceptionCodes.ValidationFailed,
            message ?? DefaultMessage,
            StatusCodes.BadRequest,
            MapErrors(failures))
    {
    }

    public ValidationAppException(IEnumerable<ErrorDetail> errors, string? message = null)
        : base(
            ExceptionCodes.ValidationFailed,
            message ?? DefaultMessage,
            StatusCodes.BadRequest,
            errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors)))
    {
    }

    private const string DefaultMessage = "Validation failed for the request.";

    private static IEnumerable<ErrorDetail> MapErrors(IEnumerable<ValidationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        return failures
            .Where(f => f is not null)
            .Select(f => new ErrorDetail(
                string.IsNullOrWhiteSpace(f.PropertyName) ? null : f.PropertyName,
                f.ErrorMessage,
                string.IsNullOrWhiteSpace(f.ErrorCode) ? null : f.ErrorCode))
            .ToArray();
    }

    private static class StatusCodes
    {
        public const int BadRequest = 400;
    }
}
