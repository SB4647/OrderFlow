namespace OrderFlow.Payments.Domain;

public sealed class Payment
{
    private Payment()
    {
    }

    private Payment(Guid id, Guid orderId, decimal amount, PaymentStatus status, DateTimeOffset processedAtUtc)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Status = status;
        ProcessedAtUtc = processedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static Payment Process(Guid id, Guid orderId, decimal amount, DateTimeOffset processedAtUtc)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount cannot be negative.");
        }

        var status = amount < 500m ? PaymentStatus.Succeeded : PaymentStatus.Failed;
        return new Payment(id, orderId, amount, status, processedAtUtc);
    }
}
