using MediatR;

namespace OrderManagement.Application.Dashboard.GetDashboardOrders;

public class GetDashboardOrdersHandler
    : IRequestHandler<GetDashboardOrdersQuery, List<DashboardOrderDto>>
{
    private readonly IDashboardReader _reader;

    public GetDashboardOrdersHandler(IDashboardReader reader)
    {
        _reader = reader;
    }

    public Task<List<DashboardOrderDto>> Handle(
        GetDashboardOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return _reader.GetOrdersAsync(cancellationToken);
    }
}
