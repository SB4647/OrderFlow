using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.UnitTests;

public sealed class OrderTests
{
    [Fact]
    public void Create_CreatesPendingOrder_WhenInputIsValid()
    {
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            orderId,
            "Demo Customer",
            [OrderItem.Create(orderId, "KB-001", 1, 99.00m)],
            DateTimeOffset.UtcNow);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(99.00m, order.Total);
    }

    [Fact]
    public void Create_RejectsEmptyOrder()
    {
        var exception = Assert.Throws<OrderValidationException>(() =>
            Order.Create(Guid.NewGuid(), "Demo Customer", [], DateTimeOffset.UtcNow));

        Assert.Contains("items", exception.Errors.Keys);
    }

    [Fact]
    public void CreateItem_RejectsInvalidQuantity()
    {
        var exception = Assert.Throws<OrderValidationException>(() =>
            OrderItem.Create(Guid.NewGuid(), "KB-001", 0, 99.00m));

        Assert.Contains("items", exception.Errors.Keys);
    }

    [Fact]
    public void Confirm_ConfirmsPendingOrder()
    {
        var order = CreateOrder();

        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Cancel_CancelsPendingOrder()
    {
        var order = CreateOrder();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    private static Order CreateOrder()
    {
        var orderId = Guid.NewGuid();
        return Order.Create(
            orderId,
            "Demo Customer",
            [OrderItem.Create(orderId, "KB-001", 1, 99.00m)],
            DateTimeOffset.UtcNow);
    }
}
