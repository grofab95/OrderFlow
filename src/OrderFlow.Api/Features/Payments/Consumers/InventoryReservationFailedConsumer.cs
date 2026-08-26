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
        var orderId = context.Message.OrderId;

        logger.LogWarning(
            "Handling InventoryReservationFailed for order {OrderId}. Reason: {Reason}. MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            orderId,
            context.Message.Reason,
            context.MessageId,
            context.CorrelationId);

        var order = await orderService.Get(orderId, context.CancellationToken);
        if (order is null)
        {
            logger.LogError(
                "Order {OrderId} was not found while handling InventoryReservationFailed",
                orderId);

            throw new InvalidOperationException($"Order with id {orderId} not found");
        }

        logger.LogDebug(
            "Cancelling order {OrderId} with status {OrderStatus}",
            orderId,
            order.Status);

        await orderService.Cancel(
            orderId,
            context.CancellationToken);
    }
}