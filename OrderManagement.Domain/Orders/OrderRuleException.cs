namespace OrderManagement.Domain.Orders;

public class OrderRuleException : Exception
{
    public OrderRuleException(string message)
        : base(message)
    {
    }
}