namespace Lynkly.Shared.Kernel.Core.Helpers.Conversion;

/// <summary>
/// Provides extension methods for enum values.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets enum member name for a value.
    /// </summary>
    public static string GetName<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        return Enum.GetName(value) ?? value.ToString();
    }

    /// <summary>
    /// Checks if the enum value is defined.
    /// </summary>
    public static bool IsDefinedValue<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        return Enum.IsDefined(value);
    }

    /// <summary>
    /// Parses a string to enum, returning a fallback when parsing fails.
    /// </summary>
    public static TEnum ParseOrDefault<TEnum>(string? value, TEnum fallback, bool ignoreCase = true)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase, out var result) ? result : fallback;
    }
}
