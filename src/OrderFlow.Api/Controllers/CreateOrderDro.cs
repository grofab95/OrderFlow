namespace OrderFlow.Api.Controllers;

public record CreateOrderRequest(IReadOnlyCollection<OrderItemDto> Items);

public sealed record OrderItemDto(int ProductId, int Quantity);