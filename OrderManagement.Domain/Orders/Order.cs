namespace OrderManagement.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Order()
    {
    }

    public Order(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.");

        customerName = customerName.Trim();

        if (customerName.Length > 100)
            throw new ArgumentException(
                "Customer name cannot exceed 100 characters.");

        Id = Guid.NewGuid();
        CustomerName = customerName;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderRuleException(
                "Only pending orders can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Process()
    {
        if (Status != OrderStatus.Confirmed)
            throw new OrderRuleException(
                "Only confirmed orders can be processed.");

        Status = OrderStatus.Processed;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending &&
            Status != OrderStatus.Confirmed)
        {
            throw new OrderRuleException(
                "Only pending or confirmed orders can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }
}