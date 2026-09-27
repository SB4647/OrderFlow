using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Messaging;

public sealed class PaymentSucceededConsumer(IOrdersService ordersService) : IConsumer<PaymentSucceeded>
{
    public Task Consume(ConsumeContext<PaymentSucceeded> context) =>
        ordersService.ProcessPaymentSucceededAsync(context.Message, context.CancellationToken);
}
