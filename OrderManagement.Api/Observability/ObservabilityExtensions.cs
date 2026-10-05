using OrderManagement.Application.Observability;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OrderManagement.Api.Observability;

public static class ObservabilityExtensions
{
    // Wires up Metrics + Tracing. Called once from Program.cs.
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        // Where traces get sent. Jaeger's all-in-one image accepts OTLP
        // on this port when started with COLLECTOR_OTLP_ENABLED=true
        // (see docker-compose.observability.yml).
        var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

        builder.Services.AddSingleton<AppMetrics>();

        // The health-check-to-metric bridge (see HealthMetricsPublisher).
        // HealthCheckPublisherOptions.Period controls how often ASP.NET
        // Core re-runs the health checks in the background.
        builder.Services.AddSingleton<HealthMetricsPublisher>();
        builder.Services.AddSingleton<IHealthCheckPublisher>(sp =>
            sp.GetRequiredService<HealthMetricsPublisher>());
        builder.Services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Period = TimeSpan.FromSeconds(10);
            options.Delay = TimeSpan.Zero;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("OrderManagement.Api"))
            .WithMetrics(metrics => metrics
                .AddMeter(AppMetrics.MeterName)
                .AddMeter("OrderManagement.Health")
                // Built-in: request count + request duration, both from
                // ONE histogram (http.server.request.duration). A
                // histogram already tracks how many observations it
                // received, so "count" and "duration" don't need two
                // separate metrics.
                .AddAspNetCoreInstrumentation()
                // Exposes everything above at GET /metrics for Prometheus
                // to scrape. See app.MapPrometheusScrapingEndpoint() in
                // Program.cs.
                .AddPrometheusExporter())
            .WithTracing(tracing => tracing
                .AddSource(AppActivitySource.Name)
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation()
                .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(otlpEndpoint)));

        return builder;
    }
}
