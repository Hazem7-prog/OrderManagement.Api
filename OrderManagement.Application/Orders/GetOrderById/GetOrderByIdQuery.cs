using MediatR;

namespace OrderManagement.Application.Orders.GetOrderById;

public class GetOrderByIdQuery : IRequest<OrderDetailsDto?>
{
    public Guid Id { get; set; }
}
