namespace Lynkly.Shared.Kernel.Core.Helpers.Core;

/// <summary>
/// Provides helper operations for <see cref="Type"/> inspection.
/// </summary>
public static class TypeHelper
{
    /// <summary>
    /// Determines whether a type is nullable.
    /// </summary>
    public static bool IsNullableType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    /// <summary>
    /// Returns default value for the supplied type.
    /// </summary>
    public static object? GetDefaultValue(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
