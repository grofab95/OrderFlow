namespace OrderFlow.Api.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    private readonly List<OrderItem> _items = [];

    protected Order()
    {
        
    }
    
    public Order(OrderItem[] items)
    {
        Id = Guid.NewGuid();
        _items.AddRange(items); 
        Status = OrderStatus.Processing;
    }
        
    public void AddItem(OrderItem item)
    {
        _items.Add(item);

        UpdateTotalAmount();
    }

    public void UpdateTotalAmount()
    {
        TotalAmount = _items.Sum(item => item.UnitPrice * item.Quantity);
    }
}