namespace OrderFlow.Orders.Application;

public interface IOrdersService
{
    Task<OrderResponse> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);

    Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderResponse>> GetRecentAsync(int take, CancellationToken cancellationToken);

    Task ProcessPaymentSucceededAsync(
        OrderFlow.Contracts.PaymentSucceeded paymentSucceeded,
        CancellationToken cancellationToken);

    Task ProcessPaymentFailedAsync(
        OrderFlow.Contracts.PaymentFailed paymentFailed,
        CancellationToken cancellationToken);
}
