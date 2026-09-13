using MediatR;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders;

public class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;

    public CreateOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = new Order(request.CustomerName);

        await _repository.AddAsync(order, cancellationToken);

        return order.Id;
    }
}