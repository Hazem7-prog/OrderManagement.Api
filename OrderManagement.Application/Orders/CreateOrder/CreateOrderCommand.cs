using System.ComponentModel.DataAnnotations;
using MediatR;

namespace OrderManagement.Application.Orders.CreateOrder;

public class CreateOrderCommand : IRequest<Guid>
{
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateOrderItem> Items { get; set; } = new();
}

public class CreateOrderItem
{
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, 1_000_000)]
    public decimal UnitPrice { get; set; }
}
