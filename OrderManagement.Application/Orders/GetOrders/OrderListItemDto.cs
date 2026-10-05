namespace OrderManagement.Application.Orders.GetOrders;

public class OrderListItemDto
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
