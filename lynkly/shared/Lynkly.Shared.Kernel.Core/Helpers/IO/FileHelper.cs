namespace Lynkly.Shared.Kernel.Core.Helpers.IO;

/// <summary>
/// Provides file system utility methods.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Ensures that the supplied directory path exists.
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Reads file content asynchronously.
    /// </summary>
    public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <summary>
    /// Writes file content asynchronously, optionally appending.
    /// </summary>
    public static Task WriteAllTextAsync(
        string path,
        string content,
        bool append = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);

        return append
            ? File.AppendAllTextAsync(path, content, cancellationToken)
            : File.WriteAllTextAsync(path, content, cancellationToken);
    }
}
