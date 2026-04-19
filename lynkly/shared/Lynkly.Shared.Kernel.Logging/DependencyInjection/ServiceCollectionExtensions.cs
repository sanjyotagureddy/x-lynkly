using Lynkly.Shared.Kernel.Logging.Abstractions;
using Lynkly.Shared.Kernel.Logging.Configuration;
using Lynkly.Shared.Kernel.Logging.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Events;

namespace Lynkly.Shared.Kernel.Logging.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelLoggingAbstractions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLogging();
        services.TryAddTransient(typeof(IStructuredLogger<>), typeof(StructuredLogger<>));

        return services;
    }

    public static IServiceCollection AddKernelLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<LoggerConfiguration>? configureLogger = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddKernelLoggingAbstractions();

        var loggingOptions = new KernelLoggingOptions();
        configuration.GetSection(KernelLoggingOptions.SectionName).Bind(loggingOptions);

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", loggingOptions.ApplicationName ?? AppDomain.CurrentDomain.FriendlyName);

        if (loggingOptions.EnableConsoleSink)
        {
            loggerConfiguration.WriteTo.Console();
        }

        if (loggingOptions.EnableFileSink)
        {
            loggerConfiguration.WriteTo.File(
                path: loggingOptions.FilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                shared: true);
        }

        configureLogger?.Invoke(loggerConfiguration);

        var logger = loggerConfiguration.CreateLogger();
        Log.Logger = logger;

        services.AddSingleton<Serilog.ILogger>(logger);
        services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));

        return services;
    }
}
