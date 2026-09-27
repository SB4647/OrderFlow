namespace OrderFlow.Payments.Application;

public interface IPaymentOutcomePublisher
{
    Task PublishAsync(PaymentProcessingResult result, CancellationToken cancellationToken);
}
