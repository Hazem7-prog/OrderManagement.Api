using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Orders;
using OrderManagement.Domain.Orders;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.BackgroundJobs;

public class OrderProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);

                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<OrdersDbContext>();

                var repository = scope.ServiceProvider
                    .GetRequiredService<IOrderRepository>();

                var orders = await context.Orders
                    .Where(order =>
                        order.Status == OrderStatus.Confirmed)
                    .OrderBy(order => order.CreatedAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var order in orders)
                {
                    order.Process();

                    await repository.SaveAsync(
                        order,
                        stoppingToken);

                    _logger.LogInformation(
                        "Processed order {OrderId}",
                        order.Id);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogInformation(
                    "An order changed during processing. " +
                    "It will be checked again on the next pass.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Order processing failed. Retrying next pass.");
            }
        }
    }
}