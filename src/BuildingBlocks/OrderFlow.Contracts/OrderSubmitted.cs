namespace OrderFlow.Contracts;

public sealed record OrderSubmitted(
    Guid MessageId,
    DateTimeOffset OccurredAtUtc,
    Guid OrderId,
    decimal Total,
    IReadOnlyCollection<OrderLine> Items);

public sealed record OrderLine(string Sku, int Quantity);
