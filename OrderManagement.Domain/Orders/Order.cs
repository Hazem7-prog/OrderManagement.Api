namespace OrderManagement.Domain.Orders;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public decimal Total { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order()
    {
    }

    public Order(string customerName, IEnumerable<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new OrderRuleException("Customer name is required.");

        customerName = customerName.Trim();

        if (customerName.Length > 100)
            throw new OrderRuleException(
                "Customer name cannot exceed 100 characters.");

        _items.AddRange(items);

        if (_items.Count == 0)
            throw new OrderRuleException(
                "An order must contain at least one item.");

        Id = Guid.NewGuid();
        CustomerName = customerName;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        Total = _items.Sum(item => item.LineTotal);
    }

    public void Complete()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderRuleException(
                "Only pending orders can be completed.");

        Status = OrderStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }
}
