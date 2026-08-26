using OrderFlow.Api.Features.Orders;

namespace OrderFlow.Api.Tests.Features.Orders;

public class OrderTests
{
    [Fact]
    public void ConstructorShouldSetPendingStatusAndCalculateTotalAmountWhenItemsAreProvided()
    {
        // Arrange
        var items = new[]
        {
            new OrderItem(Guid.NewGuid(), 2, 10.50m),
            new OrderItem(Guid.NewGuid(), 1, 5.00m)
        };

        // Act
        var order = new Order(items);

        // Assert
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(26.00m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void ConfirmShouldSetConfirmedStatusWhenOrderIsPending()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void ConfirmShouldKeepConfirmedStatusWhenCalledMoreThanOnce()
    {
        // Arrange
        var order = CreateOrder();
        order.Confirm();

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void ConfirmShouldThrowWhenOrderIsCancelled()
    {
        // Arrange
        var order = CreateOrder();
        order.Cancel();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(order.Confirm);

        // Assert
        Assert.Equal("Cannot confirm a cancelled order.", exception.Message);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void CancelShouldSetCancelledStatusWhenOrderIsPending()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void CancelShouldKeepCancelledStatusWhenCalledMoreThanOnce()
    {
        // Arrange
        var order = CreateOrder();
        order.Cancel();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void CancelShouldThrowWhenOrderIsConfirmed()
    {
        // Arrange
        var order = CreateOrder();
        order.Confirm();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(order.Cancel);

        // Assert
        Assert.Equal("Cannot cancel order with status Confirmed.", exception.Message);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    private static Order CreateOrder()
    {
        return new Order([
            new OrderItem(Guid.NewGuid(), 1, 10.00m)
        ]);
    }
}
