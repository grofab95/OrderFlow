using OrderFlow.Api.Features.Products;

namespace OrderFlow.Api.Tests.Features.Products;

public class ProductTests
{
    [Fact]
    public void ReserveShouldDecreaseQuantityWhenStockIsAvailable()
    {
        // Arrange
        var product = new Product("Test product", 10);

        // Act
        product.Reserve(3);

        // Assert
        Assert.Equal(7, product.Quantity);
    }

    [Fact]
    public void ReserveShouldThrowAndKeepQuantityWhenStockIsInsufficient()
    {
        // Arrange
        var product = new Product("Test product", 2);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => product.Reserve(3));

        // Assert
        Assert.Equal("Insufficient stock. Available: 2, requested: 3.", exception.Message);
        Assert.Equal(2, product.Quantity);
    }
}
