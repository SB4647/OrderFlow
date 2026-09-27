using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class EfPaymentsRepository(PaymentsDbContext dbContext) : IPaymentsRepository
{
    public Task<bool> IsMessageProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
        dbContext.ProcessedMessages.AnyAsync(message => message.MessageId == messageId, cancellationToken);

    public void Add(Payment payment) => dbContext.Payments.Add(payment);

    public void MarkMessageProcessed(Guid messageId, DateTimeOffset processedAtUtc) =>
        dbContext.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAtUtc = processedAtUtc
        });

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
