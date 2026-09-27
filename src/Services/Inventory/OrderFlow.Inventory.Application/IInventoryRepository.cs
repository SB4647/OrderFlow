using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Application;

public interface IInventoryRepository
{
    Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StockItem>> GetBySkusAsync(
        IReadOnlyCollection<string> skus,
        CancellationToken cancellationToken);

    void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
