using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OrderFlow.Api.Features.Inventory;
using OrderFlow.Api.Features.Inventory.Consumers;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Tests.Features.Inventory.Consumers;

public class OrderCreatedConsumerTests
{
    [Fact]
    public async Task ConsumeShouldPublishInventoryReservedWhenReservationSucceeds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var message = new OrderCreated(
            orderId,
            20.00m,
            DateTimeOffset.UtcNow,
            [new OrderCreatedItem(productId, 2)]);
        var inventoryService = Substitute.For<IInventoryService>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var context = Substitute.For<ConsumeContext<OrderCreated>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        inventoryService.TryReserve(
                orderId,
                Arg.Any<IReadOnlyCollection<InventoryReservationItem>>(),
                CancellationToken.None)
            .Returns(Task.FromResult(true));
        context.Publish(
                Arg.Any<InventoryReserved>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var consumer = new OrderCreatedConsumer(logger, inventoryService);

        // Act
        await consumer.Consume(context);

        // Assert
        await inventoryService.Received(1).TryReserve(
            orderId,
            Arg.Is<IReadOnlyCollection<InventoryReservationItem>>(items =>
                items.Count == 1 &&
                items.Single().ProductId == productId &&
                items.Single().Quantity == 2),
            CancellationToken.None);
        await context.Received(1).Publish(
            Arg.Is<InventoryReserved>(published => published.OrderId == orderId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConsumeShouldPublishInventoryReservationFailedWhenReservationFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new OrderCreated(
            orderId,
            20.00m,
            DateTimeOffset.UtcNow,
            [new OrderCreatedItem(Guid.NewGuid(), 2)]);
        var inventoryService = Substitute.For<IInventoryService>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var context = Substitute.For<ConsumeContext<OrderCreated>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        inventoryService.TryReserve(
                orderId,
                Arg.Any<IReadOnlyCollection<InventoryReservationItem>>(),
                CancellationToken.None)
            .Returns(Task.FromResult(false));
        context.Publish(
                Arg.Any<InventoryReservationFailed>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var consumer = new OrderCreatedConsumer(logger, inventoryService);

        // Act
        await consumer.Consume(context);

        // Assert
        await context.Received(1).Publish(
            Arg.Is<InventoryReservationFailed>(published => published.OrderId == orderId),
            Arg.Any<CancellationToken>());
    }
}
