namespace OrderFlow.Orders.Application;

public sealed record OrderResponse(
    Guid Id,
    string CustomerName,
    string Status,
    DateTimeOffset CreatedAtUtc,
    decimal Total,
    IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderItemResponse(string Sku, int Quantity, decimal UnitPrice);
