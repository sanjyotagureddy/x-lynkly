namespace Lynkly.Shared.Kernel.Core.Helpers.Encoding;

/// <summary>
/// Provides helper operations for text encoding and decoding.
/// </summary>
public static class EncodingHelper
{
    /// <summary>
    /// Encodes text as UTF-8 bytes.
    /// </summary>
    public static byte[] Utf8Encode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return System.Text.Encoding.UTF8.GetBytes(value);
    }

    /// <summary>
    /// Decodes UTF-8 bytes into text.
    /// </summary>
    public static string Utf8Decode(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
