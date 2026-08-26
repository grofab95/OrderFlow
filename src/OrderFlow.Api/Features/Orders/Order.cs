namespace OrderFlow.Api.Features.Orders;

public class Order
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public Order(IEnumerable<OrderItem> items)
    {
        var orderItems = items.ToArray();

        if (orderItems.Length == 0)
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        Id = Guid.NewGuid();
        Status = OrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;

        _items.AddRange(orderItems);

        TotalAmount = orderItems.Sum(x => x.UnitPrice * x.Quantity);
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Confirmed)
        {
            return;
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot confirm a cancelled order.");
        }
        
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
        {
            return;
        }

        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot cancel order with status {Status}.");
        }

        Status = OrderStatus.Cancelled;
    }
}