using MassTransit;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Inventory.Consumers;

public class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IInventoryService inventoryService) 
    : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        logger.LogInformation("Order created");
        
        var items = context.Message.Items
            .Select(x => new InventoryReservationItem(
                x.ProductId,
                x.Quantity))
            .ToArray();

        var reserved = await inventoryService.TryReserve(
            context.Message.OrderId,
            items,
            context.CancellationToken);

        if (reserved)
        {
            await context.Publish(new InventoryReserved(context.Message.OrderId));
        }
        else
        {
            await context.Publish(new InventoryReservationFailed(context.Message.OrderId, "Failed to reserve"));
        }
    }
}