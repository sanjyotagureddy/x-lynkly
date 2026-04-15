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

        throw new ValidationAppException(validationResult.Errors);
    }
}
