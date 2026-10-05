using MediatR;

namespace OrderManagement.Application.Orders.GetOrders;

public class GetOrdersQuery : IRequest<List<OrderListItemDto>>
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
