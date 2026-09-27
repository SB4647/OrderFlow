namespace OrderFlow.Orders.Domain;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    private Order(Guid id, string customerName, IReadOnlyCollection<OrderItem> items, DateTimeOffset createdAtUtc)
    {
        Id = id;
        CustomerName = customerName;
        _items.AddRange(items);
        CreatedAtUtc = createdAtUtc;
        Status = OrderStatus.Pending;
    }

    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(item => item.Quantity * item.UnitPrice);

    public static Order Create(Guid id, string customerName, IReadOnlyCollection<OrderItem>? items, DateTimeOffset createdAtUtc)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(customerName))
        {
            errors["customerName"] = ["Customer name is required."];
        }

        if (items is null)
        {
            errors["items"] = ["At least one order item is required."];
        }
        else if (items.Count == 0)
        {
            errors["items"] = ["At least one order item is required."];
        }

        if (errors.Count > 0)
        {
            throw new OrderValidationException(errors);
        }

        return new Order(id, customerName.Trim(), items!, createdAtUtc);
    }

    public void Confirm()
    {
        EnsurePending();
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        EnsurePending();
        Status = OrderStatus.Cancelled;
    }

    private void EnsurePending()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can change status.");
        }
    }
}
