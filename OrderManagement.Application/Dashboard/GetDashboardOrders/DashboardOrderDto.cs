namespace OrderManagement.Application.Dashboard.GetDashboardOrders;

public class DashboardOrderDto
{
    public Guid OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    // When this row of the read model was last rebuilt.
    public DateTime RefreshedAtUtc { get; set; }
}
