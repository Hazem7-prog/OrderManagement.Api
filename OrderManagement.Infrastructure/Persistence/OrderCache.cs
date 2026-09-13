using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Orders;
using StackExchange.Redis;

namespace OrderManagement.Infrastructure.Persistence;

public class OrderCache
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<OrderCache> _logger;

    public OrderCache(
        IDistributedCache cache,
        ILogger<OrderCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private static string Key(Guid id)
    {
        return $"order:{id:D}";
    }

    public async Task<OrderDetailsDto?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await _cache.GetStringAsync(
                Key(id),
                cancellationToken);

            if (json is null)
            {
                _logger.LogInformation("Redis MISS: {OrderId}", id);
                return null;
            }

            _logger.LogInformation("Redis HIT: {OrderId}", id);

            return JsonSerializer.Deserialize<OrderDetailsDto>(json);
        }
        catch (Exception ex)
            when (ex is RedisException || ex is TimeoutException)
        {
            _logger.LogWarning(
                ex,
                "Redis read failed; reading from SQL instead.");

            return null;
        }
    }

    public async Task SetAsync(
        OrderDetailsDto order,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.SetStringAsync(
                Key(order.Id),
                JsonSerializer.Serialize(order),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromSeconds(15)
                },
                cancellationToken);
        }
        catch (Exception ex)
            when (ex is RedisException || ex is TimeoutException)
        {
            _logger.LogWarning(ex, "Redis cache write failed.");
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        try
        {
            await _cache.RemoveAsync(Key(id));

            _logger.LogInformation(
                "Redis INVALIDATED: {OrderId}",
                id);
        }
        catch (Exception ex)
            when (ex is RedisException || ex is TimeoutException)
        {
            _logger.LogWarning(
                ex,
                "Redis invalidation failed for {OrderId}.",
                id);
        }
    }
}