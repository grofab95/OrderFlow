namespace OrderFlow.Api.Features.Orders;

public interface IOrderService
{
    Task<Order> Create(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Order?> Get(Guid id, CancellationToken cancellationToken);
    Task Confirm(Guid orderId, CancellationToken cancellationToken);
}