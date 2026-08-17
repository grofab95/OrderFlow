namespace OrderFlow.Api.Exceptions;

public class OrderNotFoundException(Guid orderId)
    : Exception($"Order not found: {orderId}.")
{
    public Guid OrderId { get; } = orderId;
}
