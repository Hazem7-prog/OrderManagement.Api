using MediatR;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Orders;

public class CreateOrderCommand : IRequest<Guid>
{
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;
}