namespace OrderManagement.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    private OrderItem()
    {
    }

    public OrderItem(string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new OrderRuleException("Product name is required.");

        if (productName.Trim().Length > 200)
            throw new OrderRuleException(
                "Product name cannot exceed 200 characters.");

        if (quantity <= 0)
            throw new OrderRuleException(
                "Quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new OrderRuleException(
                "Unit price cannot be negative.");

        Id = Guid.NewGuid();
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
