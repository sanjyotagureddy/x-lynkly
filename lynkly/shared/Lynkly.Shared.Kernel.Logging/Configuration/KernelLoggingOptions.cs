namespace Lynkly.Shared.Kernel.Logging.Configuration;

public sealed class KernelLoggingOptions
{
    public const string SectionName = "Logging:Kernel";

    public string? ApplicationName { get; set; }

    public bool EnableConsoleSink { get; set; } = true;

    public bool EnableFileSink { get; set; } = true;

    public string FilePath { get; set; } = "logs/lynkly-.log";

    public int? RetainedFileCountLimit { get; set; } = 14;
}
