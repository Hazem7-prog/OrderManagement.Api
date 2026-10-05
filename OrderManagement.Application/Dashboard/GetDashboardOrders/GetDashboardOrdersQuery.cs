using MediatR;

namespace OrderManagement.Application.Dashboard.GetDashboardOrders;

public class GetDashboardOrdersQuery : IRequest<List<DashboardOrderDto>>
{
}
