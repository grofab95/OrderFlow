using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.Features.Orders.Consumers;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Tests.Features.Orders.Consumers;

public class PaymentCompletedConsumerTests
{
    [Fact]
    public async Task ConsumeShouldConfirmOrderWhenPaymentIsCompleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderService = Substitute.For<IOrderService>();
        var logger = Substitute.For<ILogger<PaymentCompletedConsumer>>();
        var context = Substitute.For<ConsumeContext<PaymentCompleted>>();
        context.Message.Returns(new PaymentCompleted(orderId));
        context.CancellationToken.Returns(CancellationToken.None);
        orderService.Confirm(orderId, CancellationToken.None)
            .Returns(Task.CompletedTask);
        var consumer = new PaymentCompletedConsumer(logger, orderService);

        // Act
        await consumer.Consume(context);

        // Assert
        await orderService.Received(1).Confirm(orderId, CancellationToken.None);
    }
}
