namespace OrderFlow.Api.Domain.Inventories;

public record InventoryReservationItem(Guid ProductId, int Quantity);

public interface IInventoryService
{
    Task<bool> TryReserveAsync(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken);
}