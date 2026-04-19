using System.Reflection;

namespace Lynkly.Shared.Kernel.Core.Helpers.Reflection;

/// <summary>
/// Provides reflection helper utilities.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Gets a property value from an object by name.
    /// </summary>
    public static T? GetPropertyValue<T>(object source, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw SharedKernelException.InvalidOperation($"Property '{propertyName}' was not found on type '{source.GetType().Name}'.");

        var value = property.GetValue(source);

        if (value is null)
        {
            return default;
        }

        if (value is not T typedValue)
        {
            throw SharedKernelException.InvalidConversion($"Property '{propertyName}' is not of type '{typeof(T).Name}'.");
        }

        return typedValue;
    }

    /// <summary>
    /// Gets a custom attribute from a member.
    /// </summary>
    public static TAttribute? GetAttribute<TAttribute>(MemberInfo memberInfo, bool inherit = true)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(memberInfo);
        return memberInfo.GetCustomAttribute<TAttribute>(inherit);
    }

    /// <summary>
    /// Maps matching readable/writable public properties by name and assignable type.
    /// </summary>
    public static TTarget MapProperties<TSource, TTarget>(TSource source)
        where TSource : class
        where TTarget : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);

        var target = new TTarget();
        var sourceProperties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static property => property.CanRead && property.GetIndexParameters().Length == 0);
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static property => property.CanWrite && property.GetIndexParameters().Length == 0)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        foreach (var sourceProperty in sourceProperties)
        {
            if (!targetProperties.TryGetValue(sourceProperty.Name, out var targetProperty))
            {
                continue;
            }

            if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
            {
                continue;
            }

            var value = sourceProperty.GetValue(source);
            targetProperty.SetValue(target, value);
        }

        return target;
    }
}
