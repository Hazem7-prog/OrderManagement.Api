using MediatR;

namespace OrderManagement.Application.Orders;

public class ConfirmOrderCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}