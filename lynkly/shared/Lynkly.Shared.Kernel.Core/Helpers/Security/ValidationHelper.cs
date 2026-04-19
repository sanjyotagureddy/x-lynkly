namespace Lynkly.Shared.Kernel.Core.Helpers.Security;

/// <summary>
/// Provides guard-clause style validation helpers.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Throws when input is null.
    /// </summary>
    public static T AgainstNull<T>(T? value, string parameterName)
    {
        if (value is null)
        {
            throw SharedKernelException.InvalidArgument($"Argument '{parameterName}' cannot be null.");
        }

        return value;
    }

    /// <summary>
    /// Throws when string input is null, empty, or whitespace.
    /// </summary>
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw SharedKernelException.InvalidArgument($"Argument '{parameterName}' cannot be null, empty, or whitespace.");
        }

        return value;
    }

    /// <summary>
    /// Throws when numeric input is out of inclusive range.
    /// </summary>
    public static T AgainstOutOfRange<T>(T value, T minInclusive, T maxInclusive, string parameterName)
        where T : IComparable<T>
    {
        if (value.CompareTo(minInclusive) < 0 || value.CompareTo(maxInclusive) > 0)
        {
            throw SharedKernelException.InvalidArgument($"Argument '{parameterName}' must be between {minInclusive} and {maxInclusive}.");
        }

        return value;
    }
}
