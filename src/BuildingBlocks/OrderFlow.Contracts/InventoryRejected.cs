namespace OrderFlow.Contracts;

public sealed record InventoryRejected(
    Guid MessageId,
    DateTimeOffset OccurredAtUtc,
    Guid OrderId,
    string Reason);
