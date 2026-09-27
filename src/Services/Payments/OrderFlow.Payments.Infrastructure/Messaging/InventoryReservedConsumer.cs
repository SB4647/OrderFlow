using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Payments.Application;

namespace OrderFlow.Payments.Infrastructure.Messaging;

public sealed class InventoryReservedConsumer(IPaymentService paymentService) : IConsumer<InventoryReserved>
{
    public Task Consume(ConsumeContext<InventoryReserved> context) =>
        paymentService.ProcessAsync(context.Message, context.CancellationToken);
}
