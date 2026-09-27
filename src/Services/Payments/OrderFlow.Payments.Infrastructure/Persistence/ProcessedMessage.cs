namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class ProcessedMessage
{
    public Guid MessageId { get; set; }

    public DateTimeOffset ProcessedAtUtc { get; set; }
}
