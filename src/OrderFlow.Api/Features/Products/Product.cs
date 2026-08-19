namespace OrderFlow.Api.Features.Products;

public class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public int Quantity { get; private set; }
    
    public decimal Price { get; private set; }
    
    public uint Version { get; private set; }

    private Product()
    {
    }

    public Product(string name, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.");
        }

        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        Id = Guid.NewGuid();
        Name = name;
        Quantity = quantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        if (Quantity < quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {Quantity}, requested: {quantity}.");
        }

        Quantity -= quantity;
    }
}