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
            throw new ArgumentNullException(parameterName);
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
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", parameterName);
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
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {minInclusive} and {maxInclusive}.");
        }

        return value;
    }
}
