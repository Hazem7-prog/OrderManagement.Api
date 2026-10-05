using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders;

// Write side: used by Commands only.
public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task<List<Order>> GetPendingAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        Order order,
        CancellationToken cancellationToken = default);
}
