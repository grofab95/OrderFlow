using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.Features.Payments.Consumers;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Tests.Features.Payments.Consumers;

public class InventoryReservationFailedConsumerTests
{
    [Fact]
    public async Task ConsumeShouldCancelOrderWhenInventoryReservationFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderService = Substitute.For<IOrderService>();
        var logger = Substitute.For<ILogger<InventoryReservationFailedConsumer>>();
        var context = Substitute.For<ConsumeContext<InventoryReservationFailed>>();
        context.Message.Returns(new InventoryReservationFailed(orderId, "Insufficient stock."));
        context.CancellationToken.Returns(CancellationToken.None);
        orderService.Get(orderId, CancellationToken.None)
            .Returns(Task.FromResult<OrderResponse?>(new OrderResponse(
                orderId,
                OrderStatus.Pending,
                20.00m,
                DateTimeOffset.UtcNow,
                [])));
        orderService.Cancel(orderId, CancellationToken.None)
            .Returns(Task.CompletedTask);
        var consumer = new InventoryReservationFailedConsumer(logger, orderService);

        // Act
        await consumer.Consume(context);

        // Assert
        await orderService.Received(1).Cancel(orderId, CancellationToken.None);
    }
}
