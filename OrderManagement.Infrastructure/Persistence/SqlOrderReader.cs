using Dapper;
using Microsoft.Data.SqlClient;
using OrderManagement.Application.Orders;
using OrderManagement.Application.Orders.GetOrderById;
using OrderManagement.Application.Orders.GetOrders;

namespace OrderManagement.Infrastructure.Persistence;

public class SqlOrderReader : IOrderReader
{
    private readonly string _connectionString;
    private readonly OrderCache _cache;

    public SqlOrderReader(
        string connectionString,
        OrderCache cache)
    {
        _connectionString = connectionString;
        _cache = cache;
    }

    public async Task<OrderDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var cachedOrder = await _cache.GetAsync(
            id,
            cancellationToken);

        if (cachedOrder is not null)
            return cachedOrder;

        const string sql = """
            SELECT
                Id,
                CustomerName,
                Status,
                Total,
                CreatedAtUtc,
                CompletedAtUtc
            FROM dbo.Orders
            WHERE Id = @Id;

            SELECT
                ProductName,
                Quantity,
                UnitPrice
            FROM dbo.OrderItems
            WHERE OrderId = @Id;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        var sqlCommand = new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(sqlCommand);

        var order = await results
            .ReadSingleOrDefaultAsync<OrderDetailsDto>();

        if (order is null)
            return null;

        order.Items = (await results.ReadAsync<OrderItemDto>()).ToList();

        await _cache.SetAsync(order, cancellationToken);

        return order;
    }

    public async Task<List<OrderListItemDto>> GetListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                CustomerName,
                Status,
                Total,
                CreatedAtUtc
            FROM dbo.Orders
            ORDER BY CreatedAtUtc DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        var sqlCommand = new CommandDefinition(
            sql,
            new { Skip = (page - 1) * pageSize, Take = pageSize },
            cancellationToken: cancellationToken);

        var orders = await connection
            .QueryAsync<OrderListItemDto>(sqlCommand);

        return orders.ToList();
    }
}
