namespace Lynkly.Shared.Kernel.Core.Helpers.Math;

/// <summary>
/// Provides math utility methods.
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Clamps value to an inclusive range.
    /// </summary>
    public static decimal Clamp(decimal value, decimal minInclusive, decimal maxInclusive)
    {
        if (minInclusive > maxInclusive)
        {
            throw SharedKernelException.InvalidArgument("Argument 'minInclusive' cannot be greater than 'maxInclusive'.");
        }

        return System.Math.Min(System.Math.Max(value, minInclusive), maxInclusive);
    }

    /// <summary>
    /// Calculates percentage from part and total.
    /// </summary>
    public static decimal Percentage(decimal part, decimal total, int decimals = 2)
    {
        if (total == 0)
        {
            throw SharedKernelException.InvalidArgument("Argument 'total' cannot be zero when calculating percentage.");
        }

        return System.Math.Round((part / total) * 100m, decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Rounds value using selected midpoint strategy.
    /// </summary>
    public static decimal Round(decimal value, int decimals, MidpointRounding midpointRounding = MidpointRounding.AwayFromZero)
    {
        return System.Math.Round(value, decimals, midpointRounding);
    }
}
