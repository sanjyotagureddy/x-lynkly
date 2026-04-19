using FluentValidation;
using FluentValidation.Results;
using Lynkly.Shared.Kernel.Exceptions;

namespace Lynkly.Shared.Kernel.Validation;

public static class ValidatorExtensions
{
    public static void ValidateAndThrowAppException<T>(this IValidator<T> validator, T instance)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = validator.Validate(instance);
        ThrowIfInvalid(validationResult);
    }

    public static async Task ValidateAndThrowAppExceptionAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);
        ThrowIfInvalid(validationResult);
    }

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return;
        }

        throw new ValidationAppException(MapErrors(validationResult.Errors));
    }

    private static IReadOnlyList<ErrorDetail> MapErrors(IEnumerable<ValidationFailure> failures)
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
}
