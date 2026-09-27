namespace OrderFlow.Orders.Domain;

public sealed class OrderValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Order validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
