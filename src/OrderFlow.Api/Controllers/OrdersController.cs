using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Domain.Orders;

namespace OrderFlow.Api.Controllers;

public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderRequest request, 
        CancellationToken cancellationToken)
    {
        var order = await _orderService.Create(request, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.Get(id, cancellationToken);

        return Ok(order);
    }
}