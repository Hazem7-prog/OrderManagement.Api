using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderManagement.Api.Observability;
using OrderManagement.Application.Dashboard;
using OrderManagement.Application.Observability;
using OrderManagement.Application.Orders;
using OrderManagement.Application.Orders.CreateOrder;
using OrderManagement.Infrastructure.BackgroundJobs;
using OrderManagement.Infrastructure.Persistence;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging: Serilog writes structured logs to the console. ----
// ReadFrom.Configuration lets appsettings.json control the minimum
// log level without recompiling.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ---- Metrics + Tracing (OpenTelemetry). See ObservabilityExtensions. ----
builder.AddObservability();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

var redisConnectionString = builder.Configuration
    .GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "Redis connection is missing.");

// ---- Health Checks: Application + SQL Server + Redis. ----
// "application" is a trivial check: if this code runs at all, the
// process is alive and able to respond - that's the whole point of it.
builder.Services.AddHealthChecks()
    .AddCheck("application", () => HealthCheckResult.Healthy("API process is running."))
    .AddSqlServer(connectionString, name: "sqlserver")
    .AddRedis(redisConnectionString, name: "redis");

// Write side: EF Core against the transactional tables (Orders, OrderItems).
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString));

// Cache: Redis, with an expiration set per entry in OrderCache.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "OrderManagement:";
});

builder.Services.AddScoped<OrderCache>();

// Write side.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Read side: Dapper straight against SQL Server, bypassing EF change
// tracking since these are read-only queries.
builder.Services.AddScoped<IOrderReader>(services =>
    new SqlOrderReader(
        connectionString,
        services.GetRequiredService<OrderCache>()));

// Read side of the dashboard: reads ONLY from the OrderDashboard
// read table (the Materialized View), never from Orders directly.
builder.Services.AddScoped<IDashboardReader>(_ =>
    new SqlDashboardReader(connectionString));

// Rebuilds the Materialized View from the transactional tables.
builder.Services.AddScoped<IDashboardRefresher, SqlDashboardRefresher>();

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<CreateOrderCommand>());

// Background worker: completes pending orders, then refreshes the
// dashboard read model. Runs on its own timer, outside any HTTP request.
builder.Services.AddHostedService<OrderProcessingWorker>();

var app = builder.Build();

// Counts any response with a 5xx status code as an HTTP error metric.
// Placed first so it wraps every other middleware, including the
// built-in developer exception page.
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode >= 500)
    {
        var metrics = context.RequestServices.GetRequiredService<AppMetrics>();
        metrics.HttpError();
    }
});

// Structured per-request log line: method, path, status code, duration.
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Prometheus scrapes metrics from here.
app.MapPrometheusScrapingEndpoint("/metrics");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(payload));
    }
});

app.MapControllers();

app.Run();
