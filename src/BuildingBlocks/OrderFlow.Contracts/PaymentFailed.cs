namespace OrderFlow.Contracts;

public sealed record PaymentFailed(
    Guid MessageId,
    DateTimeOffset OccurredAtUtc,
    Guid OrderId);
