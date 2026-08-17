using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Domain.Orders;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(
        CreateOrderRequest request, 
        CancellationToken cancellationToken)
    {
        var order = await _orderService.Create(request, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.Get(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}