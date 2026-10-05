using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Dashboard.GetDashboardOrders;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    // FR-07: dashboard, read only from the Materialized View.
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        CancellationToken cancellationToken)
    {
        var orders = await _sender.Send(
            new GetDashboardOrdersQuery(),
            cancellationToken);

        return Ok(orders);
    }
}
