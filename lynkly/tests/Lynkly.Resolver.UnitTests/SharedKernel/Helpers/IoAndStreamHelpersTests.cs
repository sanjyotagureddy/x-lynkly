using System.Text;

using Lynkly.Shared.Kernel.Core.Helpers;
using Lynkly.Shared.Kernel.Core.Helpers.IO;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class IoAndStreamHelpersTests
{
    [Fact]
    public async Task FileHelper_Should_ReadAndWrite()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(tempDirectory, "data.txt");

        try
        {
            FileHelper.EnsureDirectoryExists(tempDirectory);
            await FileHelper.WriteAllTextAsync(filePath, "a");
            await FileHelper.WriteAllTextAsync(filePath, "b", append: true);

            var content = await FileHelper.ReadAllTextAsync(filePath);

            Assert.Equal("ab", content);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task FileHelper_Should_ValidateInput()
    {
        Assert.Throws<ArgumentException>(() => FileHelper.EnsureDirectoryExists(" "));
        await Assert.ThrowsAsync<ArgumentException>(() => FileHelper.ReadAllTextAsync(" "));
        await Assert.ThrowsAsync<ArgumentException>(() => FileHelper.WriteAllTextAsync(" ", "x"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => FileHelper.WriteAllTextAsync("/tmp/a.txt", null!));
    }

    [Fact]
    public async Task StreamHelper_Should_Work()
    {
        await using var source = new MemoryStream(Encoding.UTF8.GetBytes("lynkly"));

        var text = await StreamHelper.ReadToEndAsync(source);
        source.Position = 0;

        var bytes = await StreamHelper.ToByteArrayAsync(source);
        source.Position = 0;

        await using var copy = await StreamHelper.CopyToMemoryAsync(source);
        var copiedText = Encoding.UTF8.GetString(copy.ToArray());

        Assert.Equal("lynkly", text);
        Assert.Equal("lynkly", Encoding.UTF8.GetString(bytes));
        Assert.Equal("lynkly", copiedText);

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "file-content");
            await using var fileStream = File.OpenRead(tempFile);
            var fromFileStream = await StreamHelper.ToByteArrayAsync(fileStream);
            Assert.Equal("file-content", Encoding.UTF8.GetString(fromFileStream));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        source.Position = 3;
        var tailBytes = await StreamHelper.ToByteArrayAsync(source);
        Assert.Equal("kly", Encoding.UTF8.GetString(tailBytes));

        await Assert.ThrowsAsync<ArgumentNullException>(() => StreamHelper.ReadToEndAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => StreamHelper.ToByteArrayAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => StreamHelper.CopyToMemoryAsync(null!));
    }
}
