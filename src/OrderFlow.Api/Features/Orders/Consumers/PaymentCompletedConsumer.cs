using MassTransit;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Orders.Consumers;

public class PaymentCompletedConsumer(ILogger<PaymentCompletedConsumer> logger, IOrderService orderService)
    : IConsumer<PaymentCompleted>
{
    public async Task Consume(ConsumeContext<PaymentCompleted> context)
    {
        logger.LogInformation("Payment completed");
        
        await orderService.Confirm(context.Message.OrderId, context.CancellationToken);
    }
}