namespace OrderFlow.Orders.Api.Contracts;

public sealed record CreateOrderRequest(
    string CustomerName,
    IReadOnlyCollection<CreateOrderItemRequest>? Items);

public sealed record CreateOrderItemRequest(string Sku, int Quantity, decimal UnitPrice);
