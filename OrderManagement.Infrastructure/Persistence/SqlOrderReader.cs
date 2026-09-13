using Dapper;
using Microsoft.Data.SqlClient;
using OrderManagement.Application.Orders;

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
                CreatedAtUtc,
                ProcessedAtUtc,
                StatusLabel,
                CanConfirm
            FROM dbo.OrderDetails
            WHERE Id = @Id;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        var sqlCommand = new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken);

        var order = await connection
            .QuerySingleOrDefaultAsync<OrderDetailsDto>(sqlCommand);

        if (order is not null)
            await _cache.SetAsync(order, cancellationToken);

        return order;
    }
}