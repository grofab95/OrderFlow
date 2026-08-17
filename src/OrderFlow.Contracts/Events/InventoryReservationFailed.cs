namespace OrderFlow.Contracts.Events;

public record InventoryReservationFailed(Guid OrderId, string Reason);