namespace OrderFlow.Contracts;

public sealed record InventoryReserved(
    Guid MessageId,
    DateTimeOffset OccurredAtUtc,
    Guid OrderId);
