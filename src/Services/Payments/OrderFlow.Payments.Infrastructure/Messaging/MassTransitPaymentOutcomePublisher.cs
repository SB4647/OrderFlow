using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Payments.Application;

namespace OrderFlow.Payments.Infrastructure.Messaging;

public sealed class MassTransitPaymentOutcomePublisher(IPublishEndpoint publishEndpoint) : IPaymentOutcomePublisher
{
    public Task PublishAsync(PaymentProcessingResult result, CancellationToken cancellationToken)
    {
        if (result.IsSucceeded)
        {
            return publishEndpoint.Publish(
                new PaymentSucceeded(Guid.NewGuid(), DateTimeOffset.UtcNow, result.OrderId),
                cancellationToken);
        }

        return publishEndpoint.Publish(
            new PaymentFailed(Guid.NewGuid(), DateTimeOffset.UtcNow, result.OrderId),
            cancellationToken);
    }
}
