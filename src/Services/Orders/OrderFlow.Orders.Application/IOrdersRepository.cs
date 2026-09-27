using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Application;

public interface IOrdersRepository
{
    void Add(Order order);

    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetRecentAsync(
        int take,
        Guid? createdByUserId,
        CancellationToken cancellationToken);

    Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken);

    void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
