using OrderFlow.Contracts;

namespace OrderFlow.Orders.Application;

public interface IOrderSubmittedPublisher
{
    Task PublishAsync(OrderSubmitted orderSubmitted, CancellationToken cancellationToken);
}
