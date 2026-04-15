using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Lynkly.Shared.Kernel.Helpers;

/// <summary>
/// Provides security helper operations.
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// Computes SHA-256 hash in lowercase hexadecimal format.
    /// </summary>
    public static string ComputeSha256(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Encodes text to Base64.
    /// </summary>
    public static string ToBase64(string input, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert.ToBase64String((encoding ?? Encoding.UTF8).GetBytes(input));
    }

    /// <summary>
    /// Decodes Base64 text.
    /// </summary>
    public static string FromBase64(string input, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        return (encoding ?? Encoding.UTF8).GetString(Convert.FromBase64String(input));
    }

    /// <summary>
    /// Performs constant-time comparison for two strings.
    /// </summary>
    public static bool FixedTimeEquals(string? left, string? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        var maxLength = Math.Max(leftBytes.Length, rightBytes.Length);
        var comparisonLength = maxLength + sizeof(int);
        var leftComparisonBytes = new byte[comparisonLength];
        var rightComparisonBytes = new byte[comparisonLength];

        Buffer.BlockCopy(leftBytes, 0, leftComparisonBytes, 0, leftBytes.Length);
        Buffer.BlockCopy(rightBytes, 0, rightComparisonBytes, 0, rightBytes.Length);
        BinaryPrimitives.WriteInt32LittleEndian(leftComparisonBytes.AsSpan(maxLength), leftBytes.Length);
        BinaryPrimitives.WriteInt32LittleEndian(rightComparisonBytes.AsSpan(maxLength), rightBytes.Length);

        return CryptographicOperations.FixedTimeEquals(leftComparisonBytes, rightComparisonBytes);
    }
}
