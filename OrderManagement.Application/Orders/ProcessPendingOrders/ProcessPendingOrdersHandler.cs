using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Observability;

namespace OrderManagement.Application.Orders.ProcessPendingOrders;

public class ProcessPendingOrdersHandler
    : IRequestHandler<ProcessPendingOrdersCommand, int>
{
    private const int BatchSize = 20;

    private readonly IOrderRepository _repository;
    private readonly AppMetrics _metrics;
    private readonly ILogger<ProcessPendingOrdersHandler> _logger;

    public ProcessPendingOrdersHandler(
        IOrderRepository repository,
        AppMetrics metrics,
        ILogger<ProcessPendingOrdersHandler> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<int> Handle(
        ProcessPendingOrdersCommand request,
        CancellationToken cancellationToken)
    {
        _metrics.WorkerRun();

        var orders = await _repository.GetPendingAsync(
            BatchSize,
            cancellationToken);

        foreach (var order in orders)
        {
            order.Complete();

            // SaveAsync also removes the order from the Redis cache.
            await _repository.SaveAsync(order, cancellationToken);

            _logger.LogInformation(
                "Worker completed order {OrderId}",
                order.Id);
        }

        _metrics.OrdersCompleted(orders.Count);

        return orders.Count;
    }
}
