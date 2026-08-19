using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OrderFlow.Api.Caching;
using OrderFlow.Api.Features.Products;
using OrderFlow.Api.Persistence;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Api.Features.Orders;

public class OrderService : IOrderService
{
    private static readonly JsonSerializerOptions CacheJsonOptions = new(JsonSerializerDefaults.Web);
    
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public OrderService(AppDbContext dbContext, IPublishEndpoint publishEndpoint, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
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

    public async Task<OrderResponse?> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = OrderCacheKeys.ById(id);
        var cachedJson = await _cache.GetStringAsync(
            cacheKey,
            cancellationToken);

        if (cachedJson is not null)
        {
            return JsonSerializer.Deserialize<OrderResponse>(
                cachedJson,
                CacheJsonOptions);
        }

        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (order is null)
        {
            return null;
        }

        var response = new OrderResponse(
            order.Id,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.Items
                .Select(x => new OrderItemResponse(
                    x.ProductId,
                    x.Quantity,
                    x.UnitPrice))
                .ToArray());

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(response, CacheJsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken);

        return response;
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
        await _cache.RemoveAsync(OrderCacheKeys.ById(order.Id), cancellationToken);
    }

    public async Task Cancel(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(orderId);
        }

        order.Cancel();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}