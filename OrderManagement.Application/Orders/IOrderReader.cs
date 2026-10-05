using OrderManagement.Application.Orders.GetOrderById;
using OrderManagement.Application.Orders.GetOrders;

namespace OrderManagement.Application.Orders;

// Read side: used by Queries only.
public interface IOrderReader
{
    Task<OrderDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<OrderListItemDto>> GetListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
