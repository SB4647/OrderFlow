using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Messaging;

public sealed class PaymentFailedConsumer(IOrdersService ordersService) : IConsumer<PaymentFailed>
{
    public Task Consume(ConsumeContext<PaymentFailed> context) =>
        ordersService.ProcessPaymentFailedAsync(context.Message, context.CancellationToken);
}
