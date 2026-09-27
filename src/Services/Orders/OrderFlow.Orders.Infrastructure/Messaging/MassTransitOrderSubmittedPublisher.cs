using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Messaging;

public sealed class MassTransitOrderSubmittedPublisher(IPublishEndpoint publishEndpoint) : IOrderSubmittedPublisher
{
    public Task PublishAsync(OrderSubmitted orderSubmitted, CancellationToken cancellationToken) =>
        publishEndpoint.Publish(orderSubmitted, cancellationToken);
}
