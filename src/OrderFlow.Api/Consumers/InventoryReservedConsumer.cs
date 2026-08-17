using MassTransit;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Consumers;

public class InventoryReservedConsumer(ILogger<InventoryReservedConsumer> logger) : IConsumer<InventoryReserved>
{
    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        logger.LogInformation("Inventory reserved");
        
        await Task.Delay(TimeSpan.FromSeconds(15));
        
        await context.Publish(new PaymentCompleted(context.Message.OrderId));
    }
}