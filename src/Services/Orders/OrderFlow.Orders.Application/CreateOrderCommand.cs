namespace OrderFlow.Orders.Application;

public sealed record CreateOrderCommand(
    Guid CreatedByUserId,
    string CustomerName,
    IReadOnlyCollection<CreateOrderItemCommand>? Items);

public sealed record CreateOrderItemCommand(string Sku, int Quantity, decimal UnitPrice);
