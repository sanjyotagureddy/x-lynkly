namespace Lynkly.Shared.Kernel.Core.Helpers.Text;

/// <summary>
/// Provides commonly used string extension methods.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns <see langword="null"/> when the value is null, empty, or whitespace.
    /// </summary>
    public static string? ToNullIfWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Truncates the value to the provided maximum length.
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length cannot be negative.");
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>
    /// Performs an ordinal case-insensitive equality comparison.
    /// </summary>
    public static bool EqualsOrdinalIgnoreCase(this string? value, string? other)
    {
        return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
    }
}
