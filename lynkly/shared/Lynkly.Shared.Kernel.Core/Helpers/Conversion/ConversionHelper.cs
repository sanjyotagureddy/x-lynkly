using System.Globalization;

namespace Lynkly.Shared.Kernel.Core.Helpers.Conversion;

/// <summary>
/// Provides safe conversion helper methods.
/// </summary>
public static class ConversionHelper
{
    /// <summary>
    /// Converts value to target type or returns fallback.
    /// </summary>
    public static T ConvertOrDefault<T>(object? value, T fallback = default!, IFormatProvider? formatProvider = null)
    {
        return TryConvert(value, out T? converted, formatProvider) ? converted! : fallback;
    }

    /// <summary>
    /// Attempts to convert value to target type.
    /// </summary>
    public static bool TryConvert<T>(object? value, out T? converted, IFormatProvider? formatProvider = null)
    {
        if (value is null)
        {
            converted = default;
            return false;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        var isNullableTarget = Nullable.GetUnderlyingType(typeof(T)) is not null;

        try
        {
            if (targetType.IsEnum)
            {
                if (value is string enumString && Enum.TryParse(targetType, enumString, true, out var enumResult))
                {
                    converted = ConvertEnum<T>(enumResult, targetType, isNullableTarget);
                    return true;
                }

                if (value.GetType().IsPrimitive)
                {
                    var numericValue = Convert.ChangeType(value, Enum.GetUnderlyingType(targetType), formatProvider ?? CultureInfo.InvariantCulture);
                    converted = ConvertEnum<T>(Enum.ToObject(targetType, numericValue!), targetType, isNullableTarget);
                    return true;
                }
            }

            var result = Convert.ChangeType(value, targetType, formatProvider ?? CultureInfo.InvariantCulture);
            converted = isNullableTarget
                ? (T?)(object)result!
                : (T?)result;
            return true;
        }
        catch (FormatException)
        {
            converted = default;
            return false;
        }
        catch (InvalidCastException)
        {
            converted = default;
            return false;
        }
        catch (OverflowException)
        {
            converted = default;
            return false;
        }
    }

    private static T ConvertEnum<T>(object enumValue, Type underlyingEnumType, bool isNullableTarget)
    {
        if (!isNullableTarget)
        {
            return (T)enumValue;
        }

        var nullableType = typeof(Nullable<>).MakeGenericType(underlyingEnumType);
        var nullableValue = Activator.CreateInstance(nullableType, enumValue)
            ?? throw new InvalidCastException($"Cannot convert value to nullable enum type '{nullableType.Name}'.");
        return (T)nullableValue;
    }
}
