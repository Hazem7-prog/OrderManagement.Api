using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Orders;
using OrderManagement.Infrastructure.BackgroundJobs;
using OrderManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration
        .GetConnectionString("Redis")
        ?? throw new InvalidOperationException(
            "Redis connection is missing.");

    options.InstanceName = "OrderManagement:";
});

builder.Services.AddScoped<OrderCache>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IOrderReader>(services =>
    new SqlOrderReader(
        connectionString,
        services.GetRequiredService<OrderCache>()));

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<CreateOrderCommand>());

builder.Services.AddHostedService<OrderProcessingWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Order Management API");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();