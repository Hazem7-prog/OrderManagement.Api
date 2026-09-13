namespace OrderManagement.Application.Orders;

public interface IOrderReader
{
    Task<OrderDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}