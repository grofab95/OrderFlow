namespace OrderFlow.Api.Features.Orders;

public record CreateOrderRequest(IReadOnlyCollection<OrderItemDto> Items);

public sealed record OrderItemDto(Guid ProductId, int Quantity);