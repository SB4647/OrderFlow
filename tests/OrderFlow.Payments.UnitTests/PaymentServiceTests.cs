using OrderFlow.Contracts;
using OrderFlow.Payments.Application;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.UnitTests;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task ProcessAsync_PublishesSucceeded_WhenTotalIsBelow500()
    {
        var repository = new FakePaymentsRepository();
        var publisher = new FakePaymentOutcomePublisher();
        var service = new PaymentService(repository, publisher);

        await service.ProcessAsync(CreateInventoryReserved(499.99m), CancellationToken.None);

        Assert.Single(repository.Payments);
        Assert.Equal(PaymentStatus.Succeeded, repository.Payments[0].Status);
        Assert.True(publisher.Results.Single().IsSucceeded);
    }

    [Fact]
    public async Task ProcessAsync_PublishesFailed_WhenTotalIs500OrMore()
    {
        var repository = new FakePaymentsRepository();
        var publisher = new FakePaymentOutcomePublisher();
        var service = new PaymentService(repository, publisher);

        await service.ProcessAsync(CreateInventoryReserved(500.00m), CancellationToken.None);

        Assert.Single(repository.Payments);
        Assert.Equal(PaymentStatus.Failed, repository.Payments[0].Status);
        Assert.False(publisher.Results.Single().IsSucceeded);
    }

    [Fact]
    public async Task ProcessAsync_IgnoresDuplicateInventoryReservation()
    {
        var repository = new FakePaymentsRepository();
        var publisher = new FakePaymentOutcomePublisher();
        var service = new PaymentService(repository, publisher);
        var reservation = CreateInventoryReserved(100m);

        await service.ProcessAsync(reservation, CancellationToken.None);
        await service.ProcessAsync(reservation, CancellationToken.None);

        Assert.Single(repository.Payments);
        Assert.Single(publisher.Results);
    }

    private static InventoryReserved CreateInventoryReserved(decimal total) =>
        new(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), total);

    private sealed class FakePaymentsRepository : IPaymentsRepository
    {
        private readonly HashSet<Guid> _processedMessageIds = [];

        public List<Payment> Payments { get; } = [];

        public Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
            Task.FromResult(_processedMessageIds.Contains(messageId));

        public void Add(Payment payment) => Payments.Add(payment);

        public void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc) =>
            _processedMessageIds.Add(messageId);

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakePaymentOutcomePublisher : IPaymentOutcomePublisher
    {
        public List<PaymentProcessingResult> Results { get; } = [];

        public Task PublishAsync(PaymentProcessingResult result, CancellationToken cancellationToken)
        {
            Results.Add(result);
            return Task.CompletedTask;
        }
    }
}
