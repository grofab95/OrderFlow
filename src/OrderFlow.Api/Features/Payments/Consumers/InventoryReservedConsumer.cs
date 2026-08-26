using MassTransit;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Payments.Consumers;

public class InventoryReservedConsumer(ILogger<InventoryReservedConsumer> logger) : IConsumer<InventoryReserved>
{
    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        var orderId = context.Message.OrderId;

        logger.LogInformation(
            "Handling InventoryReserved for order {OrderId}. MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            orderId,
            context.MessageId,
            context.CorrelationId);

        await Task.Delay(TimeSpan.FromSeconds(15), context.CancellationToken);

        await context.Publish(new PaymentCompleted(orderId));

        logger.LogDebug("Published PaymentCompleted for order {OrderId}", orderId);
    }
}