using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Inventory.Application;

namespace OrderFlow.Inventory.Infrastructure.Messaging;

public sealed class OrderSubmittedConsumer(
    IInventoryService inventoryService,
    IPublishEndpoint publishEndpoint) : IConsumer<OrderSubmitted>
{
    public async Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var result = await inventoryService.ReserveAsync(context.Message, context.CancellationToken);
        if (result is null)
        {
            return;
        }

        if (result.IsReserved)
        {
            await publishEndpoint.Publish(
                new InventoryReserved(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    context.Message.OrderId,
                    context.Message.Total),
                context.CancellationToken);
            return;
        }

        await publishEndpoint.Publish(
            new InventoryRejected(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                context.Message.OrderId,
                result.RejectionReason ?? "Inventory reservation failed."),
            context.CancellationToken);
    }
}
