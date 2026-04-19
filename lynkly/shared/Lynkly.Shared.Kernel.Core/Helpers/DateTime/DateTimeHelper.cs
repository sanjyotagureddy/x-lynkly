namespace Lynkly.Shared.Kernel.Core.Helpers.DateTime;

/// <summary>
/// Provides helper operations for UTC-based date and time handling.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Ensures the provided <see cref="DateTime"/> is represented in UTC.
    /// </summary>
    public static System.DateTime EnsureUtc(System.DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => System.DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Converts the provided <see cref="DateTimeOffset"/> to Unix time seconds.
    /// </summary>
    public static long ToUnixTimeSeconds(DateTimeOffset value)
    {
        return value.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts Unix time seconds to a UTC <see cref="DateTimeOffset"/>.
    /// </summary>
    public static DateTimeOffset FromUnixTimeSeconds(long unixSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
    }

    /// <summary>
    /// Returns the UTC start-of-day for the provided date/time.
    /// </summary>
    public static System.DateTime StartOfDayUtc(System.DateTime value)
    {
        var utc = EnsureUtc(value);
        return new System.DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Returns the UTC end-of-day for the provided date/time.
    /// </summary>
    public static System.DateTime EndOfDayUtc(System.DateTime value)
    {
        return StartOfDayUtc(value).AddDays(1).AddTicks(-1);
    }
}
