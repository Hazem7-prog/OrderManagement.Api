using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Orders;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Infrastructure.Persistence;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;
    private readonly OrderCache _cache;

    public OrderRepository(
        OrdersDbContext context,
        OrderCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);

        await SaveAsync(order, cancellationToken);
    }

    public Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Orders.SingleOrDefaultAsync(
            order => order.Id == id,
            cancellationToken);
    }

    public async Task SaveAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(order.Id);
    }
}