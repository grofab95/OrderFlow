using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderFlow.Api.Features.Inventory;
using OrderFlow.Api.Features.Products;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Tests.Features.Inventory;

public class InventoryServiceTests
{
    [Fact]
    public async Task TryReserveShouldDecreaseStockOnlyOnceWhenOrderIsProcessedMoreThanOnce()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var databaseRoot = new InMemoryDatabaseRoot();
        var product = new Product("Test product", 10);
        var orderId = Guid.NewGuid();
        var items = new[]
        {
            new InventoryReservationItem(product.Id, 3)
        };

        await using (var arrangeDbContext = CreateDbContext(databaseName, databaseRoot))
        {
            arrangeDbContext.Products.Add(product);
            await arrangeDbContext.SaveChangesAsync();
        }

        // Act
        bool firstReservationResult;
        await using (var firstDbContext = CreateDbContext(databaseName, databaseRoot))
        {
            var service = new InventoryService(firstDbContext);
            firstReservationResult = await service.TryReserve(orderId, items, CancellationToken.None);
        }

        bool secondReservationResult;
        await using (var secondDbContext = CreateDbContext(databaseName, databaseRoot))
        {
            var service = new InventoryService(secondDbContext);
            secondReservationResult = await service.TryReserve(orderId, items, CancellationToken.None);
        }

        // Assert
        await using var assertDbContext = CreateDbContext(databaseName, databaseRoot);
        var savedProduct = await assertDbContext.Products.SingleAsync(x => x.Id == product.Id);
        var reservationCount = await assertDbContext.InventoryReservations
            .CountAsync(x => x.OrderId == orderId);

        Assert.True(firstReservationResult);
        Assert.True(secondReservationResult);
        Assert.Equal(7, savedProduct.Quantity);
        Assert.Equal(1, reservationCount);
    }

    [Fact]
    public async Task TryReserveShouldKeepStockUnchangedWhenStockIsInsufficient()
    {
        // Arrange
        await using var dbContext = CreateDbContext(
            Guid.NewGuid().ToString(),
            new InMemoryDatabaseRoot());
        var product = new Product("Test product", 2);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var service = new InventoryService(dbContext);

        // Act
        var result = await service.TryReserve(
            Guid.NewGuid(),
            [new InventoryReservationItem(product.Id, 3)],
            CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(2, product.Quantity);
        Assert.Empty(dbContext.InventoryReservations);
    }

    private static AppDbContext CreateDbContext(
        string databaseName,
        InMemoryDatabaseRoot databaseRoot)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName, databaseRoot)
            .Options;

        return new AppDbContext(options);
    }
}
