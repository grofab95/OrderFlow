using OrderFlow.Api.Controllers;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Domain.Orders;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Order> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var orderItems = request.Items
            .Select(x => new OrderItem(x.ProductId, x.Quantity))
            .ToArray();
        
        var order = new Order(orderItems);

        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.OrderItems.AddRangeAsync(order.Items, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public Task<Order?> Get(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}