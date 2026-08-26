using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Features.Inventory;

public class InventoryService(
    AppDbContext dbContext,
    ILogger<InventoryService> logger) : IInventoryService
{
    public async Task<bool> TryReserve(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            LogRejected(orderId, "No items requested");

            return false;
        }

        if (await dbContext.InventoryReservations
                .AnyAsync(x => x.OrderId == orderId, cancellationToken))
        {
            logger.LogDebug(
                "Inventory for order {OrderId} is already reserved, skipping duplicate reservation",
                orderId);

            return true;
        }

        var requestedQuantities = items
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(x => x.Quantity));

        if (requestedQuantities.Values.Any(quantity => quantity <= 0))
        {
            LogRejected(orderId, "Non-positive quantity requested");

            return false;
        }

        var productIds = requestedQuantities.Keys.ToArray();

        var products = await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(
                product => product.Id,
                cancellationToken);

        if (products.Count != productIds.Length)
        {
            LogRejected(orderId, "Unknown product");

            return false;
        }

        var insufficientStock = requestedQuantities.Any(request =>
            products[request.Key].Quantity < request.Value);

        if (insufficientStock)
        {
            LogRejected(orderId, "Insufficient stock");

            return false;
        }

        foreach (var request in requestedQuantities)
        {
            var product = products[request.Key];

            product.Reserve(request.Value);
        }
        
        var inventoryReservation = new InventoryReservation(orderId);

        await dbContext.InventoryReservations.AddAsync(inventoryReservation, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Inventory was reserved for order {OrderId} across {ProductCount} product(s)",
            orderId,
            requestedQuantities.Count);

        return true;
    }

    private void LogRejected(Guid orderId, string reason)
    {
        logger.LogWarning(
            "Inventory reservation for order {OrderId} was rejected. Reason: {Reason}",
            orderId,
            reason);
    }
}