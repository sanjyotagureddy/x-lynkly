namespace Lynkly.Shared.Kernel.Core.Helpers.Core;

/// <summary>
/// Provides generic helper operations.
/// </summary>
public static class GeneralHelper
{
    /// <summary>
    /// Returns the first non-null value from a sequence.
    /// </summary>
    public static T? Coalesce<T>(params T?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (var value in values)
        {
            if (value is not null)
            {
                return value;
            }
        }

        return default;
    }

    /// <summary>
    /// Swaps two values.
    /// </summary>
    public static void Swap<T>(ref T left, ref T right)
    {
        (left, right) = (right, left);
    }
}
