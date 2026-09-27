using OrderFlow.Contracts;
using OrderFlow.Inventory.Application;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.UnitTests;

public sealed class InventoryServiceTests
{
    [Fact]
    public async Task ReserveAsync_ReservesStock_WhenStockIsAvailable()
    {
        var stockItem = new StockItem("KB-001", 2);
        var repository = new FakeInventoryRepository(stockItem);
        var service = new InventoryService(repository);

        var result = await service.ReserveAsync(CreateOrderSubmitted(Guid.NewGuid(), 1), CancellationToken.None);

        Assert.True(result!.IsReserved);
        Assert.Equal(1, stockItem.AvailableQuantity);
        Assert.Equal(1, repository.ProcessedMessageCount);
    }

    [Fact]
    public async Task ReserveAsync_IgnoresDuplicateMessage_WithoutReservingStockAgain()
    {
        var stockItem = new StockItem("KB-001", 2);
        var repository = new FakeInventoryRepository(stockItem);
        var service = new InventoryService(repository);
        var message = CreateOrderSubmitted(Guid.NewGuid(), 1);

        await service.ReserveAsync(message, CancellationToken.None);
        var duplicateResult = await service.ReserveAsync(message, CancellationToken.None);

        Assert.Null(duplicateResult);
        Assert.Equal(1, stockItem.AvailableQuantity);
        Assert.Equal(1, repository.ProcessedMessageCount);
    }

    private static OrderSubmitted CreateOrderSubmitted(Guid messageId, int quantity) =>
        new(messageId, DateTimeOffset.UtcNow, Guid.NewGuid(), [new OrderLine("KB-001", quantity)]);

    private sealed class FakeInventoryRepository(StockItem stockItem) : IInventoryRepository
    {
        private readonly HashSet<Guid> _processedMessageIds = [];

        public int ProcessedMessageCount => _processedMessageIds.Count;

        public Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
            Task.FromResult(_processedMessageIds.Contains(messageId));

        public Task<IReadOnlyList<StockItem>> GetBySkusAsync(
            IReadOnlyCollection<string> skus,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<StockItem>>(
                skus.Contains(stockItem.Sku) ? [stockItem] : []);

        public void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc) =>
            _processedMessageIds.Add(messageId);

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
