using System.Security.Cryptography;

namespace Lynkly.Shared.Kernel.Core.Helpers.Math;

/// <summary>
/// Provides pseudo-random and cryptographically secure random utilities.
/// </summary>
public static class RandomHelper
{
    /// <summary>
    /// Returns a pseudo-random integer in the specified range.
    /// </summary>
    public static int NextInt(int minInclusive, int maxExclusive)
    {
        return Random.Shared.Next(minInclusive, maxExclusive);
    }

    /// <summary>
    /// Returns pseudo-random bytes with the provided length.
    /// </summary>
    public static byte[] NextBytes(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
        }

        var bytes = new byte[length];
        Random.Shared.NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// Returns cryptographically secure random bytes with the provided length.
    /// </summary>
    public static byte[] NextSecureBytes(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
        }

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    /// <summary>
    /// Returns a cryptographically secure random integer in the specified range.
    /// </summary>
    public static int NextSecureInt(int minInclusive, int maxExclusive)
    {
        return RandomNumberGenerator.GetInt32(minInclusive, maxExclusive);
    }
}
