using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Controllers;
using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Persistence;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Domain.Orders;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task<Order> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var productIds = request.Items
            .Select(x => x.ProductId)
            .Distinct()
            .ToArray();

        var products = await _dbContext.Products
            .Where(x => productIds.AsEnumerable().Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var missingProductIds = productIds
            .Where(id => !products.ContainsKey(id))
            .ToArray();

        if (missingProductIds.Length > 0)
        {
            throw new ProductNotFoundException(missingProductIds);
        }

        var orderItems = request.Items
            .Select(x =>
            {
                var product = products[x.ProductId];

                return new OrderItem(
                    product.Id,
                    x.Quantity,
                    product.Price);
            })
            .ToArray();

        var order = new Order(orderItems);

        _dbContext.Orders.Add(order);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new OrderCreated(
                order.Id,
                order.TotalAmount,
                order.CreatedAt,
                order.Items
                    .Select(x => new OrderCreatedItem(
                        x.ProductId,
                        x.Quantity))
                    .ToArray()),
            cancellationToken);

        return order;
    }

    public Task<Order?> Get(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task Confirm(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(orderId);
        }

        order.Confirm();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}