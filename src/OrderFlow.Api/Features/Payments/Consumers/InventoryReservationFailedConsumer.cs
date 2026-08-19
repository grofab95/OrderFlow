using MassTransit;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Payments.Consumers;

public class InventoryReservationFailedConsumer(
    ILogger<InventoryReservationFailedConsumer> logger,
    IOrderService orderService) : IConsumer<InventoryReservationFailed>
{
    public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
    {
        var order = await orderService.Get(context.Message.OrderId, context.CancellationToken);
        if (order is null)
        {
            throw new InvalidOperationException($"Order with id {context.Message.OrderId} not found");
        }
        
        await orderService.Cancel(
            context.Message.OrderId,
            context.CancellationToken);
    }
}