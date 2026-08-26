using MassTransit;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Inventory.Consumers;

public class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IInventoryService inventoryService) 
    : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var orderId = context.Message.OrderId;

        var items = context.Message.Items
            .Select(x => new InventoryReservationItem(
                x.ProductId,
                x.Quantity))
            .ToArray();

        logger.LogInformation(
            "Handling OrderCreated for order {OrderId} with {ItemCount} item(s). MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            orderId,
            items.Length,
            context.MessageId,
            context.CorrelationId);

        var reserved = await inventoryService.TryReserve(
            orderId,
            items,
            context.CancellationToken);

        if (reserved)
        {
            await context.Publish(new InventoryReserved(orderId));

            logger.LogDebug("Published InventoryReserved for order {OrderId}", orderId);
        }
        else
        {
            await context.Publish(new InventoryReservationFailed(orderId, "Failed to reserve"));

            logger.LogWarning(
                "Inventory could not be reserved for order {OrderId}, published InventoryReservationFailed",
                orderId);
        }
    }
}