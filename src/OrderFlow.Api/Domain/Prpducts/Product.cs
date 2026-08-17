namespace OrderFlow.Api.Domain.Prpducts;

public class Product
{
    public Guid Id { get; private set; }
    public required string Name { get; init; }
    public int AvailableQuantity { get; init; }
}