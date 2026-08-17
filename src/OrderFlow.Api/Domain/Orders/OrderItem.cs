namespace OrderFlow.Api.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { } // EF Core

    public OrderItem(int productId, int quantity)
    {
        // validation

        ProductId = productId;
        Quantity = quantity;
    }
}