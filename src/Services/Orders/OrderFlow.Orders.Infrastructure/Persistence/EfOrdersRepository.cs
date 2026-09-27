using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Infrastructure.Persistence;

public sealed class EfOrdersRepository(OrdersDbContext dbContext) : IOrdersRepository
{
    public void Add(Order order) => dbContext.Orders.Add(order);

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Orders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetRecentAsync(
        int take,
        Guid? createdByUserId,
        CancellationToken cancellationToken)
    {
        var orders = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAtUtc)
            .AsQueryable();

        if (createdByUserId is not null)
        {
            orders = orders.Where(order => order.CreatedByUserId == createdByUserId);
        }

        return await orders.Take(take).ToListAsync(cancellationToken);
    }

    public Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
        dbContext.ProcessedMessages.AnyAsync(message => message.MessageId == messageId, cancellationToken);

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
