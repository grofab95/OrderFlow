namespace OrderFlow.Api.Features.Orders;

public interface IOrderService
{
    Task<OrderResponse> Create(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> Get(Guid id, CancellationToken cancellationToken);
    Task Confirm(Guid orderId, CancellationToken cancellationToken);
    Task Cancel(Guid orderId, CancellationToken cancellationToken);
}