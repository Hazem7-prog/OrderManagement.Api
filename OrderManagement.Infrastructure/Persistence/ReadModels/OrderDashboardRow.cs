namespace OrderManagement.Infrastructure.Persistence.ReadModels;

// The Materialized View: a normal SQL table that holds pre-computed
// dashboard rows. It is a READ MODEL rebuilt from Orders + OrderItems.
// It is never the source of truth and is never written by API requests.
public class OrderDashboardRow
{
    public Guid OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime RefreshedAtUtc { get; set; }
}
