using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Dashboard;

namespace OrderManagement.Infrastructure.Persistence;

// Rebuilds the OrderDashboard read table from the transactional tables.
// Runs in one transaction so readers never see a half-empty dashboard.
public class SqlDashboardRefresher : IDashboardRefresher
{
    private readonly OrdersDbContext _context;

    public SqlDashboardRefresher(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.OrderDashboard;",
            cancellationToken);

        await _context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OrderDashboard
                (OrderId, CustomerName, ItemCount, Total,
                 Status, CreatedAtUtc, RefreshedAtUtc)
            SELECT
                o.Id,
                o.CustomerName,
                COUNT(i.Id),
                o.Total,
                o.Status,
                o.CreatedAtUtc,
                SYSUTCDATETIME()
            FROM dbo.Orders o
            LEFT JOIN dbo.OrderItems i ON i.OrderId = o.Id
            GROUP BY o.Id, o.CustomerName, o.Total, o.Status, o.CreatedAtUtc;
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
