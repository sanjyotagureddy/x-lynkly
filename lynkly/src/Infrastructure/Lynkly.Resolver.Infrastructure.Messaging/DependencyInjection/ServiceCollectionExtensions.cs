using Lynkly.Resolver.Infrastructure.Messaging.Configuration;
using Lynkly.Resolver.Infrastructure.Messaging.Internal;
using Lynkly.Shared.Kernel.Messaging.Abstractions;
using Lynkly.Shared.Kernel.Messaging.Configuration;
using Lynkly.Shared.Kernel.Messaging.DependencyInjection;
using Lynkly.Shared.Kernel.Persistence.DependencyInjection;
using Lynkly.Shared.Kernel.Logging.DependencyInjection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Lynkly.Resolver.Infrastructure.Messaging.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddKernelMessaging();
        services.AddKernelPersistence();
        services.AddKernelLoggingAbstractions();

        EnsureSupportedTransport(configuration);
        ConfigureMessagingOptions(services, configuration);
        services.Replace(ServiceDescriptor.Scoped<IMessagePublisher, MassTransitMessagePublisher>());
        ConfigureMassTransit(services, configuration);

        return services;
    }

    private static void EnsureSupportedTransport(IConfiguration configuration)
    {
        var configuredMessagingOptions =
            configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>() ?? new MessagingOptions();
        if (configuredMessagingOptions.Broker != MessagingTransportKind.RabbitMq)
        {
            throw new NotSupportedException(
                $"Messaging broker '{configuredMessagingOptions.Broker}' is not supported by this infrastructure module.");
        }
    }

    private static void ConfigureMessagingOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MessagingOptions>()
            .Bind(configuration.GetSection(MessagingOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                "Messaging connection string name must be configured.")
            .ValidateOnStart();

        services.AddOptions<RabbitMqMessagingOptions>()
            .Bind(configuration.GetSection(RabbitMqMessagingOptions.SectionName))
            .Validate(options => options.PublishRetryCount > 0,
                "Messaging:RabbitMq:PublishRetryCount must be greater than zero.")
            .Validate(options => options.MinRetryDelay > TimeSpan.Zero,
                "Messaging:RabbitMq:MinRetryDelay must be greater than zero.")
            .Validate(options => options.MaxRetryDelay >= options.MinRetryDelay,
                "Messaging:RabbitMq:MaxRetryDelay must be greater than or equal to MinRetryDelay.")
            .Validate(options => options.IntervalDelta > TimeSpan.Zero,
                "Messaging:RabbitMq:IntervalDelta must be greater than zero.")
            .ValidateOnStart();
    }

    private static void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.ConfigureHealthCheckOptions(options =>
            {
                options.Name = "lynkly-rabbitmq";
                options.Tags.Add("ready");
            });
            configurator.UsingRabbitMq((context, rabbitMqConfiguration) =>
            {
                var connectionString = ResolveRabbitMqConnectionString(context, configuration);
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqMessagingOptions>>().Value;

                rabbitMqConfiguration.Host(new Uri(connectionString));
                rabbitMqConfiguration.UseMessageRetry(retryConfiguration =>
                    retryConfiguration.Exponential(
                        rabbitMqOptions.PublishRetryCount,
                        rabbitMqOptions.MinRetryDelay,
                        rabbitMqOptions.MaxRetryDelay,
                        rabbitMqOptions.IntervalDelta));
                rabbitMqConfiguration.ConfigureEndpoints(context);
            });
        });
    }

    private static string ResolveRabbitMqConnectionString(IBusRegistrationContext context, IConfiguration configuration)
    {
        var messagingOptions = context.GetRequiredService<IOptions<MessagingOptions>>().Value;
        var rabbitMqConnectionString = configuration.GetConnectionString(messagingOptions.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{messagingOptions.ConnectionStringName}' was not found.");
        }

        return rabbitMqConnectionString;
    }
}
