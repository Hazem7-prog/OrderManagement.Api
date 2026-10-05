using MediatR;

namespace OrderManagement.Application.Dashboard.RefreshDashboard;

public class RefreshDashboardHandler
    : IRequestHandler<RefreshDashboardCommand>
{
    private readonly IDashboardRefresher _refresher;

    public RefreshDashboardHandler(IDashboardRefresher refresher)
    {
        _refresher = refresher;
    }

    public Task Handle(
        RefreshDashboardCommand request,
        CancellationToken cancellationToken)
    {
        return _refresher.RefreshAsync(cancellationToken);
    }
}
