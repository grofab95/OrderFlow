using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Features.Inventory;

public class InventoryService(AppDbContext dbContext) : IInventoryService
{
    public async Task<bool> TryReserveAsync(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return false;
        }

        var requestedQuantities = items
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(x => x.Quantity));

        if (requestedQuantities.Values.Any(quantity => quantity <= 0))
        {
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
            return false;
        }

        var insufficientStock = requestedQuantities.Any(request =>
            products[request.Key].Quantity < request.Value);

        if (insufficientStock)
        {
            return false;
        }

        foreach (var request in requestedQuantities)
        {
            var product = products[request.Key];

            product.Reserve(request.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}