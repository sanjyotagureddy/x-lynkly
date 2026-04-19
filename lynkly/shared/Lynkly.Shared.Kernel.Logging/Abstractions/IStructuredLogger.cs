using Microsoft.Extensions.Logging;

namespace Lynkly.Shared.Kernel.Logging.Abstractions;

public interface IStructuredLogger<TCategory>
{
    IDisposable BeginScope(IReadOnlyDictionary<string, object?> properties);

    void LogInformation(string messageTemplate, params object?[] propertyValues);

    void LogWarning(string messageTemplate, params object?[] propertyValues);

    void LogError(Exception exception, string messageTemplate, params object?[] propertyValues);

    bool IsEnabled(LogLevel logLevel);
}
