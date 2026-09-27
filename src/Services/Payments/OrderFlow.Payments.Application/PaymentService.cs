using OrderFlow.Contracts;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.Application;

public sealed class PaymentService(
    IPaymentsRepository paymentsRepository,
    IPaymentOutcomePublisher paymentOutcomePublisher) : IPaymentService
{
    public async Task ProcessAsync(InventoryReserved inventoryReserved, CancellationToken cancellationToken)
    {
        if (await paymentsRepository.IsMessageProcessedAsync(inventoryReserved.MessageId, cancellationToken))
        {
            return;
        }

        var payment = Payment.Process(
            Guid.NewGuid(),
            inventoryReserved.OrderId,
            inventoryReserved.Total,
            DateTimeOffset.UtcNow);

        paymentsRepository.Add(payment);
        paymentsRepository.MarkMessageProcessed(inventoryReserved.MessageId, payment.ProcessedAtUtc);
        await paymentsRepository.SaveChangesAsync(cancellationToken);
        await paymentOutcomePublisher.PublishAsync(
            new PaymentProcessingResult(
                payment.OrderId,
                payment.Amount,
                payment.Status == PaymentStatus.Succeeded),
            cancellationToken);
    }
}
