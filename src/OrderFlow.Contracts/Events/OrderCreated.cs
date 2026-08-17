namespace OrderFlow.Contracts.Events;

public record OrderCreated(
    Guid OrderId,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<OrderCreatedItem> Items);

public record OrderCreatedItem(
    Guid ProductId,
    int Quantity);