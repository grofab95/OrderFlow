namespace OrderFlow.Api.Features.Orders;

public sealed record OrderResponse(
    Guid Id,
    OrderStatus Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);