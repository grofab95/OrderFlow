namespace OrderFlow.Api.Features.Inventory;

public interface IInventoryService
{
    Task<bool> TryReserveAsync(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken);
}
