using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class EfInventoryRepository(InventoryDbContext dbContext) : IInventoryRepository
{
    public Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
        dbContext.ProcessedMessages.AnyAsync(message => message.MessageId == messageId, cancellationToken);

    public async Task<IReadOnlyList<StockItem>> GetBySkusAsync(
        IReadOnlyCollection<string> skus,
        CancellationToken cancellationToken) =>
        await dbContext.StockItems
            .Where(item => skus.Contains(item.Sku))
            .ToListAsync(cancellationToken);

    public void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc) =>
        dbContext.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAtUtc = processedAtUtc
        });

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
