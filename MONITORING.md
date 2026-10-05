# OrderFlow - Monitoring & Observability

## What was added
- **Health Checks**: `GET /health` -> JSON with `application`, `sqlserver`, `redis` each reported separately.
- **Logging**: Serilog, structured console logs for order creation, retrieval, cache hit/miss (already existed in `OrderCache.cs`), worker activity, and unhandled errors (via the framework's own exception logging).
- **Metrics**: `GET /metrics` (Prometheus format). See `OrderManagement.Application/Observability/AppMetrics.cs` for the metric list and why each type (Counter / UpDownCounter / Histogram) was chosen.
- **Tracing**: spans for CreateOrder, GetOrderById, GetOrders, exported via OTLP to Jaeger, nested under the automatic ASP.NET Core + SQL Client spans.
- **Grafana dashboard + 2 alerts**: provisioned automatically from `observability/grafana/provisioning/`.

## Run the monitoring stack
```
docker compose -f docker-compose.observability.yml up -d
```
- Prometheus: http://localhost:9090
- Grafana:    http://localhost:3000  (admin / admin)
- Jaeger UI:  http://localhost:16686

Then run the API as usual (`dotnet run` from `OrderManagement.Api/`) and open Swagger to create/read orders.

## Test Scenario 1 - Normal
1. Create a few orders via Swagger, read a few back.
2. Open Grafana -> OrderFlow folder -> the dashboard. Confirm all panels show data.
3. Open Jaeger -> service `OrderManagement.Api` -> find a `CreateOrder` or `GetOrderById` trace.
4. Watch the API's console for structured log lines.

## Test Scenario 2 - Dependency failure
1. Stop Redis (`docker stop ordermanagement-redis`) or SQL Server.
2. Call any endpoint that touches it.
3. Check `/health` (that dependency now shows `Unhealthy`), the console logs (a `fail`/`warn` entry), the `dependency_up` metric in Grafana, and a trace showing the failed call.
