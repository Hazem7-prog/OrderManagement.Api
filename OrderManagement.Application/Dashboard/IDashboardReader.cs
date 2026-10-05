using OrderManagement.Application.Dashboard.GetDashboardOrders;

namespace OrderManagement.Application.Dashboard;

// Reads from the Materialized View (read table), never from Orders.
public interface IDashboardReader
{
    Task<List<DashboardOrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken = default);
}
