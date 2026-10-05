using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderManagement.Application.Observability;

// Every time ASP.NET Core runs the registered health checks (SQL Server,
// Redis), it calls PublishAsync here. We remember the latest result per
// dependency and expose it as a Gauge metric ("dependency_up"), so
// Grafana/Prometheus can alert on a dependency being down - not just
// show it on the /health page.
public class HealthMetricsPublisher : IHealthCheckPublisher
{
    private static readonly Meter Meter = new("OrderManagement.Health", "1.0.0");

    private readonly ConcurrentDictionary<string, int> _latestStatus = new();

    public HealthMetricsPublisher()
    {
        Meter.CreateObservableGauge(
            "dependency_up",
            ObserveDependencies,
            description: "1 if the dependency's last health check was Healthy, 0 otherwise.");
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        foreach (var entry in report.Entries)
        {
            _latestStatus[entry.Key] = entry.Value.Status == HealthStatus.Healthy ? 1 : 0;
        }

        return Task.CompletedTask;
    }

    private IEnumerable<Measurement<int>> ObserveDependencies()
    {
        foreach (var (name, isUp) in _latestStatus)
        {
            yield return new Measurement<int>(
                isUp,
                new KeyValuePair<string, object?>("name", name));
        }
    }
}
