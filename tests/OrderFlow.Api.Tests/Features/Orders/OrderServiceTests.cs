using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using OrderFlow.Api.Caching;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Tests.Features.Orders;

public class OrderServiceTests
{
    [Fact]
    public async Task ConfirmShouldRemoveCacheEntryWhenOrderIsConfirmed()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        var cache = Substitute.For<IDistributedCache>();
        cache.RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var service = CreateService(dbContext, cache);

        // Act
        await service.Confirm(order.Id, CancellationToken.None);

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        await cache.Received(1).RemoveAsync(
            OrderCacheKeys.ById(order.Id),
            CancellationToken.None);
    }

    [Fact]
    public async Task CancelShouldRemoveCacheEntryWhenOrderIsCancelled()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        var cache = Substitute.For<IDistributedCache>();
        cache.RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var service = CreateService(dbContext, cache);

        // Act
        await service.Cancel(order.Id, CancellationToken.None);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        await cache.Received(1).RemoveAsync(
            OrderCacheKeys.ById(order.Id),
            CancellationToken.None);
    }

    private static OrderService CreateService(
        AppDbContext dbContext,
        IDistributedCache cache)
    {
        return new OrderService(
            dbContext,
            Substitute.For<IPublishEndpoint>(),
            cache);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Order CreateOrder()
    {
        return new Order([
            new OrderItem(Guid.NewGuid(), 1, 10.00m)
        ]);
    }
}
