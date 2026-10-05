using Dapper;
using Microsoft.Data.SqlClient;
using OrderManagement.Application.Dashboard;
using OrderManagement.Application.Dashboard.GetDashboardOrders;

namespace OrderManagement.Infrastructure.Persistence;

// Reads ONLY from the OrderDashboard read table (the Materialized View).
public class SqlDashboardReader : IDashboardReader
{
    private readonly string _connectionString;

    public SqlDashboardReader(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<DashboardOrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                OrderId,
                CustomerName,
                ItemCount,
                Total,
                Status,
                RefreshedAtUtc
            FROM dbo.OrderDashboard
            ORDER BY CreatedAtUtc DESC;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        var sqlCommand = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<DashboardOrderDto>(sqlCommand);

        return rows.ToList();
    }
}
