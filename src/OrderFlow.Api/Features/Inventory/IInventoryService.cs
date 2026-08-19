namespace OrderFlow.Api.Features.Inventory;

public interface IInventoryService
{
    Task<bool> TryReserve(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken);
}
