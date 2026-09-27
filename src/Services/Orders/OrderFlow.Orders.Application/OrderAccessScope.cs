namespace OrderFlow.Orders.Application;

public sealed record OrderAccessScope(Guid UserId, bool CanViewAllOrders);
