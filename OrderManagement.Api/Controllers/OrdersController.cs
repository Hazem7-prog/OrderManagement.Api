using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Orders;
using Microsoft.EntityFrameworkCore;
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

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var orderId = await _sender.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new { id = orderId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery
        {
            Id = id
        };

        var order = await _sender.Send(query, cancellationToken);

        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(
    Guid id,
    CancellationToken cancellationToken)
    {
        try
        {
            var found = await _sender.Send(
                new ConfirmOrderCommand
                {
                    Id = id
                },
                cancellationToken);

            if (!found)
                return NotFound();

            return NoContent();
        }
        catch (OrderRuleException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message =
                    "The order changed during this request. Reload and retry."
            });
        }
    }
}