using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.Application;

public interface IPaymentsRepository
{
    Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken);

    void Add(Payment payment);

    void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
