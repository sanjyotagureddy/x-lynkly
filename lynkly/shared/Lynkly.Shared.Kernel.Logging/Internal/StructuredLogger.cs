using Lynkly.Shared.Kernel.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Lynkly.Shared.Kernel.Logging.Internal;

internal sealed class StructuredLogger<TCategory>(ILogger<TCategory> logger) : IStructuredLogger<TCategory>
{
    private readonly ILogger<TCategory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public IDisposable BeginScope(IReadOnlyDictionary<string, object?> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return _logger.BeginScope(properties);
    }

    public void LogInformation(string messageTemplate, params object?[] propertyValues)
    {
        _logger.LogInformation(messageTemplate, propertyValues);
    }

    public void LogWarning(string messageTemplate, params object?[] propertyValues)
    {
        _logger.LogWarning(messageTemplate, propertyValues);
    }

    public void LogError(Exception exception, string messageTemplate, params object?[] propertyValues)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _logger.LogError(exception, messageTemplate, propertyValues);
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);
}
