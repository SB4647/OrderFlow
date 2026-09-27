namespace OrderFlow.Contracts;

public sealed record PaymentSucceeded(
    Guid MessageId,
    DateTimeOffset OccurredAtUtc,
    Guid OrderId);
