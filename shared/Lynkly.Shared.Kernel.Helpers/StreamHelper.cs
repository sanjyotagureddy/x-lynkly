using System.Text;

namespace Lynkly.Shared.Kernel.Helpers;

/// <summary>
/// Provides stream helper operations.
/// </summary>
public static class StreamHelper
{
    /// <summary>
    /// Reads stream content as text asynchronously.
    /// </summary>
    public static async Task<string> ReadToEndAsync(
        Stream stream,
        Encoding? encoding = null,
        bool leaveOpen = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: leaveOpen);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads stream into a byte array asynchronously.
    /// </summary>
    public static async Task<byte[]> ToByteArrayAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream is MemoryStream memoryStream)
        {
            if (!memoryStream.CanSeek)
            {
                return memoryStream.ToArray();
            }

            if (memoryStream.Position >= memoryStream.Length)
            {
                return [];
            }

            var bytes = memoryStream.ToArray();
            var start = checked((int)memoryStream.Position);
            var count = checked((int)(memoryStream.Length - memoryStream.Position));
            var remainingBytes = new byte[count];
            Buffer.BlockCopy(bytes, start, remainingBytes, 0, count);
            return remainingBytes;
        }

        using var target = new MemoryStream();
        await stream.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
        return target.ToArray();
    }

    /// <summary>
    /// Copies a stream to a new memory stream asynchronously.
    /// </summary>
    public static async Task<MemoryStream> CopyToMemoryAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var target = new MemoryStream();
        await stream.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
        target.Position = 0;
        return target;
    }
}
