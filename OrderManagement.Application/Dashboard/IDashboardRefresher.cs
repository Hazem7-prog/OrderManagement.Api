namespace OrderManagement.Application.Dashboard;

// Rebuilds the Materialized View from the transactional tables.
public interface IDashboardRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
