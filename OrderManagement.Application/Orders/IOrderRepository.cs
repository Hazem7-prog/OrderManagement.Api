using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders;

public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        Order order,
        CancellationToken cancellationToken = default);
}