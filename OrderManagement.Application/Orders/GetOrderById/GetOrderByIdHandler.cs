using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Observability;

namespace OrderManagement.Application.Orders.GetOrderById;

public class GetOrderByIdHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDetailsDto?>
{
    private readonly IOrderReader _reader;
    private readonly ILogger<GetOrderByIdHandler> _logger;

    public GetOrderByIdHandler(
        IOrderReader reader,
        ILogger<GetOrderByIdHandler> logger)
    {
        _reader = reader;
        _logger = logger;
    }

    public async Task<OrderDetailsDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        using var activity = AppActivitySource.Source.StartActivity("GetOrderById");
        activity?.SetTag("order.id", request.Id);

        var order = await _reader.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Order {OrderId} was requested but does not exist",
                request.Id);
        }
        else
        {
            _logger.LogInformation(
                "Order {OrderId} retrieved with status {Status}",
                order.Id,
                order.Status);
        }

        return order;
    }
}
