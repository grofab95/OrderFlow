namespace OrderFlow.Api.Features.Inventory;

public class InventoryReservation
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private InventoryReservation()
    {

    }
    
    public InventoryReservation(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID cannot be empty", nameof(orderId));
        }
        
        Id = Guid.NewGuid();
        OrderId = orderId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}