using OrderFlow.Contracts;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.UnitTests;

public sealed class PaymentResultProcessingTests
{
    [Fact]
    public async Task ProcessPaymentSucceededAsync_ConfirmsPendingOrder()
    {
        var order = CreateOrder();
        var repository = new FakeOrdersRepository(order);
        var service = new OrdersService(repository, new NoOpOrderSubmittedPublisher());

        await service.ProcessPaymentSucceededAsync(
            new PaymentSucceeded(Guid.NewGuid(), DateTimeOffset.UtcNow, order.Id),
            CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(1, repository.ProcessedMessageCount);
    }

    [Fact]
    public async Task ProcessPaymentFailedAsync_CancelsPendingOrder()
    {
        var order = CreateOrder();
        var repository = new FakeOrdersRepository(order);
        var service = new OrdersService(repository, new NoOpOrderSubmittedPublisher());

        await service.ProcessPaymentFailedAsync(
            new PaymentFailed(Guid.NewGuid(), DateTimeOffset.UtcNow, order.Id),
            CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(1, repository.ProcessedMessageCount);
    }

    private static Order CreateOrder()
    {
        var orderId = Guid.NewGuid();
        return Order.Create(
            orderId,
            Guid.NewGuid(),
            "Demo Customer",
            [OrderItem.Create(orderId, "KB-001", 1, 99.00m)],
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeOrdersRepository(Order order) : IOrdersRepository
    {
        private readonly HashSet<Guid> _processedMessageIds = [];

        public int ProcessedMessageCount => _processedMessageIds.Count;

        public void Add(Order addedOrder)
        {
        }

        public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken) =>
            Task.FromResult(order.Id == orderId ? order : null);

        public Task<IReadOnlyList<Order>> GetRecentAsync(
            int take,
            Guid? createdByUserId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Order>>([order]);

        public Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
            Task.FromResult(_processedMessageIds.Contains(messageId));

        public void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc) =>
            _processedMessageIds.Add(messageId);

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpOrderSubmittedPublisher : IOrderSubmittedPublisher
    {
        public Task PublishAsync(OrderSubmitted orderSubmitted, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
