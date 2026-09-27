using OrderFlow.Contracts;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Application;

public sealed class OrdersService(
    IOrdersRepository ordersRepository,
    IOrderSubmittedPublisher orderSubmittedPublisher) : IOrdersService
{
    public async Task<OrderResponse> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.Items is null)
        {
            throw new OrderValidationException(new Dictionary<string, string[]>
            {
                ["items"] = ["At least one order item is required."]
            });
        }

        var orderId = Guid.NewGuid();
        var items = command.Items
            .Select(item => OrderItem.Create(orderId, item.Sku, item.Quantity, item.UnitPrice))
            .ToArray();
        var order = Order.Create(orderId, command.CustomerName, items, DateTimeOffset.UtcNow);

        ordersRepository.Add(order);
        await ordersRepository.SaveChangesAsync(cancellationToken);
        await orderSubmittedPublisher.PublishAsync(
            new OrderSubmitted(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                order.Id,
                order.Total,
                order.Items.Select(item => new OrderLine(item.Sku, item.Quantity)).ToArray()),
            cancellationToken);

        return Map(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await ordersRepository.GetByIdAsync(orderId, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetRecentAsync(int take, CancellationToken cancellationToken)
    {
        if (take is < 1 or > 100)
        {
            throw new OrderValidationException(new Dictionary<string, string[]>
            {
                ["take"] = ["Take must be between 1 and 100."]
            });
        }

        var orders = await ordersRepository.GetRecentAsync(take, cancellationToken);
        return orders.Select(Map).ToArray();
    }

    public Task ProcessPaymentSucceededAsync(
        PaymentSucceeded paymentSucceeded,
        CancellationToken cancellationToken) =>
        ApplyPaymentResultAsync(
            paymentSucceeded.MessageId,
            paymentSucceeded.OrderId,
            succeeded: true,
            cancellationToken);

    public Task ProcessPaymentFailedAsync(
        PaymentFailed paymentFailed,
        CancellationToken cancellationToken) =>
        ApplyPaymentResultAsync(
            paymentFailed.MessageId,
            paymentFailed.OrderId,
            succeeded: false,
            cancellationToken);

    private async Task ApplyPaymentResultAsync(
        Guid messageId,
        Guid orderId,
        bool succeeded,
        CancellationToken cancellationToken)
    {
        if (await ordersRepository.IsMessageProcessedAsync(messageId, cancellationToken))
        {
            return;
        }

        var order = await ordersRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{orderId}' was not found for payment processing.");

        if (succeeded)
        {
            order.Confirm();
        }
        else
        {
            order.Cancel();
        }

        ordersRepository.MarkMessageProcessed(messageId, DateTimeOffset.UtcNow);
        await ordersRepository.SaveChangesAsync(cancellationToken);
    }

    private static OrderResponse Map(Order order) => new(
        order.Id,
        order.CustomerName,
        order.Status.ToString(),
        order.CreatedAtUtc,
        order.Total,
        order.Items.Select(item => new OrderItemResponse(item.Sku, item.Quantity, item.UnitPrice)).ToArray());
}
