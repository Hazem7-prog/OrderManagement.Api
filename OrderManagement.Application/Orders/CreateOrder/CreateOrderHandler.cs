using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Observability;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders.CreateOrder;

public class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;
    private readonly AppMetrics _metrics;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository repository,
        AppMetrics metrics,
        ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        using var activity = AppActivitySource.Source.StartActivity("CreateOrder");

        var items = request.Items.Select(item =>
            new OrderItem(
                item.ProductName,
                item.Quantity,
                item.UnitPrice));

        // The Order calculates its own total and enforces its own rules.
        var order = new Order(request.CustomerName, items);

        activity?.SetTag("order.id", order.Id);
        activity?.SetTag("order.total", order.Total);
        activity?.SetTag("order.item_count", order.Items.Count);

        await _repository.AddAsync(order, cancellationToken);

        _metrics.OrderCreated();
        _metrics.OrderBecamePending();

        _logger.LogInformation(
            "Order {OrderId} created for {CustomerName} with total {Total}",
            order.Id,
            order.CustomerName,
            order.Total);

        return order.Id;
    }
}
