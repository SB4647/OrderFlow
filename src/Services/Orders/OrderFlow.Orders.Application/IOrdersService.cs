namespace OrderFlow.Orders.Application;

public interface IOrdersService
{
    Task<OrderResponse> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);

    Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderResponse>> GetRecentAsync(int take, CancellationToken cancellationToken);
}
