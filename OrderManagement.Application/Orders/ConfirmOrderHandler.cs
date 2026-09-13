using MediatR;

namespace OrderManagement.Application.Orders;

public class ConfirmOrderHandler
    : IRequestHandler<ConfirmOrderCommand, bool>
{
    private readonly IOrderRepository _repository;

    public ConfirmOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        ConfirmOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (order is null)
            return false;

        order.Confirm();

        await _repository.SaveAsync(order, cancellationToken);

        return true;
    }
}