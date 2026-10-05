using MediatR;

namespace OrderManagement.Application.Orders.ProcessPendingOrders;

// Returns how many orders were moved from Pending to Completed.
public class ProcessPendingOrdersCommand : IRequest<int>
{
}
