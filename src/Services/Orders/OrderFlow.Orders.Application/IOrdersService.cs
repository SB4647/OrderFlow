namespace OrderFlow.Orders.Application;

public interface IOrdersService
{
    Task<OrderResponse> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);

    Task<OrderResponse?> GetByIdAsync(
        Guid orderId,
        OrderAccessScope accessScope,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderResponse>> GetRecentAsync(
        int take,
        OrderAccessScope accessScope,
        CancellationToken cancellationToken);

    Task ProcessPaymentSucceededAsync(
        OrderFlow.Contracts.PaymentSucceeded paymentSucceeded,
        CancellationToken cancellationToken);

    Task ProcessPaymentFailedAsync(
        OrderFlow.Contracts.PaymentFailed paymentFailed,
        CancellationToken cancellationToken);
}
