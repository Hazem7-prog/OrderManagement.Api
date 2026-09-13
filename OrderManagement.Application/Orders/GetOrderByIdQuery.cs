using MediatR;

namespace OrderManagement.Application.Orders;

public class GetOrderByIdQuery : IRequest<OrderDetailsDto?>
{
    public Guid Id { get; set; }
}   