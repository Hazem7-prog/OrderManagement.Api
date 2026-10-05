using System.Diagnostics.Metrics;

namespace OrderManagement.Application.Observability;

// One Meter for the whole app. Registered as a Singleton so every
// handler shares the same counters instead of creating new ones.
//
// Metric types used here, and why:
//   - Counter        -> a number that only ever goes UP (orders created,
//                        worker runs, http errors). Prometheus/Grafana
//                        read these with rate()/increase().
//   - UpDownCounter   -> a number that goes UP and DOWN (pending orders:
//                        +1 when created, -1 when the worker completes
//                        it). This is what Prometheus exposes as a Gauge.
public class AppMetrics
{
    public const string MeterName = "OrderManagement";

    private readonly Counter<long> _ordersCreated;
    private readonly UpDownCounter<int> _ordersPending;
    private readonly Counter<long> _httpErrors;
    private readonly Counter<long> _workerRuns;
    private readonly Counter<long> _workerOrdersCompleted;

    public AppMetrics()
    {
        var meter = new Meter(MeterName, "1.0.0");

        _ordersCreated = meter.CreateCounter<long>(
            "orders_created_total",
            description: "Total number of orders created.");

        _ordersPending = meter.CreateUpDownCounter<int>(
            "orders_pending",
            description: "Current number of orders waiting to be processed by the worker.");

        _httpErrors = meter.CreateCounter<long>(
            "http_errors_total",
            description: "Total number of HTTP responses with a 5xx status code.");

        _workerRuns = meter.CreateCounter<long>(
            "worker_runs_total",
            description: "Total number of times the background worker has executed a pass.");

        _workerOrdersCompleted = meter.CreateCounter<long>(
            "worker_orders_completed_total",
            description: "Total number of orders completed by the background worker.");
    }

    public void OrderCreated() => _ordersCreated.Add(1);

    public void OrderBecamePending() => _ordersPending.Add(1);

    public void OrdersCompleted(int count)
    {
        if (count <= 0)
            return;

        _ordersPending.Add(-count);
        _workerOrdersCompleted.Add(count);
    }

    public void HttpError() => _httpErrors.Add(1);

    public void WorkerRun() => _workerRuns.Add(1);
}
