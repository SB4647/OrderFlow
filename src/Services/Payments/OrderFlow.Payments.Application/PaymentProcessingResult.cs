namespace OrderFlow.Payments.Application;

public sealed record PaymentProcessingResult(Guid OrderId, decimal Amount, bool IsSucceeded);
