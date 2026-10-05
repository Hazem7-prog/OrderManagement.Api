using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Orders.CreateOrder;
using OrderManagement.Application.Orders.GetOrderById;
using OrderManagement.Application.Orders.GetOrders;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    // FR-01, FR-02, FR-03, FR-04: create an order with items and a total.
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var orderId = await _sender.Send(command, cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new { id = orderId });
        }
        catch (OrderRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // FR-05: order details by id, served through the cache.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery { Id = id };

        var order = await _sender.Send(query, cancellationToken);

        if (order is null)
            return NotFound();

        return Ok(order);
    }

    // FR-06: list of orders for an admin/read screen.
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery
        {
            Page = page == 0 ? 1 : page,
            PageSize = pageSize == 0 ? 20 : pageSize
        };

        var orders = await _sender.Send(query, cancellationToken);

        return Ok(orders);
    }
}
