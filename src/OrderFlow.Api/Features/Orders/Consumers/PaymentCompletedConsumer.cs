using MassTransit;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Orders.Consumers;

public class PaymentCompletedConsumer(ILogger<PaymentCompletedConsumer> logger, IOrderService orderService)
    : IConsumer<PaymentCompleted>
{
    public async Task Consume(ConsumeContext<PaymentCompleted> context)
    {
        var orderId = context.Message.OrderId;

        logger.LogInformation(
            "Handling PaymentCompleted for order {OrderId}. MessageId: {MessageId}, CorrelationId: {CorrelationId}",
            orderId,
            context.MessageId,
            context.CorrelationId);

        await orderService.Confirm(orderId, context.CancellationToken);
    }
}