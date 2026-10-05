using MediatR;
using OrderManagement.Application.Observability;

namespace OrderManagement.Application.Orders.GetOrders;

public class GetOrdersHandler
    : IRequestHandler<GetOrdersQuery, List<OrderListItemDto>>
{
    private readonly IOrderReader _reader;

    public GetOrdersHandler(IOrderReader reader)
    {
        _reader = reader;
    }

    public Task<List<OrderListItemDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        using var activity = AppActivitySource.Source.StartActivity("GetOrders");

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        activity?.SetTag("page", page);
        activity?.SetTag("page_size", pageSize);

        return _reader.GetListAsync(page, pageSize, cancellationToken);
    }
}
