using MassTransit;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lynkly.Resolver.Infrastructure.Messaging.HealthChecks;

internal sealed class RabbitMqBusHealthCheck(IBusHealth busHealth) : IHealthCheck
{
    private readonly IBusHealth _busHealth = busHealth ?? throw new ArgumentNullException(nameof(busHealth));

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var busHealthResult = _busHealth.CheckHealth();

        return Task.FromResult(busHealthResult.Status switch
        {
            BusHealthStatus.Healthy => HealthCheckResult.Healthy("RabbitMQ bus is healthy."),
            BusHealthStatus.Degraded => HealthCheckResult.Degraded("RabbitMQ bus is degraded."),
            _ => HealthCheckResult.Unhealthy("RabbitMQ bus is unhealthy.")
        });
    }
}
