using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Dashboard.RefreshDashboard;
using OrderManagement.Application.Orders.ProcessPendingOrders;

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

                // The worker is a Singleton, but the DbContext and the
                // handlers are Scoped, so each pass creates its own scope.
                await using var scope =
                    _scopeFactory.CreateAsyncScope();

                var sender = scope.ServiceProvider
                    .GetRequiredService<ISender>();

                var completed = await sender.Send(
                    new ProcessPendingOrdersCommand(),
                    stoppingToken);

                if (completed > 0)
                {
                    _logger.LogInformation(
                        "Completed {Count} pending order(s)",
                        completed);
                }

                await sender.Send(
                    new RefreshDashboardCommand(),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Background pass failed. Retrying next pass.");
            }
        }
    }
}
