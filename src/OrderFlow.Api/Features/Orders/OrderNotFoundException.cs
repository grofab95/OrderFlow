namespace OrderFlow.Api.Features.Orders;

public class OrderNotFoundException(Guid orderId)
    : Exception($"Order not found: {orderId}.")
{
    public Guid OrderId { get; } = orderId;
}
