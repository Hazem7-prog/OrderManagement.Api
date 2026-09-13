using MediatR;

namespace OrderManagement.Application.Orders;

public class GetOrderByIdHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDetailsDto?>
{
    private readonly IOrderReader _reader;

    public GetOrderByIdHandler(IOrderReader reader)
    {
        _reader = reader;
    }

    public async Task<OrderDetailsDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _reader.GetByIdAsync(
            request.Id,
            cancellationToken);
    }
}