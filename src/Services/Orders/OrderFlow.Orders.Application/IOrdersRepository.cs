using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Application;

public interface IOrdersRepository
{
    void Add(Order order);

    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetRecentAsync(int take, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
