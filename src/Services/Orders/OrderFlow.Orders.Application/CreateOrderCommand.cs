namespace OrderFlow.Orders.Application;

public sealed record CreateOrderCommand(
    string CustomerName,
    IReadOnlyCollection<CreateOrderItemCommand>? Items);

public sealed record CreateOrderItemCommand(string Sku, int Quantity, decimal UnitPrice);
