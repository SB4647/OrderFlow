namespace OrderFlow.Orders.Domain;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(Guid id, Guid orderId, string sku, int quantity, decimal unitPrice)
    {
        Id = id;
        OrderId = orderId;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public static OrderItem Create(Guid orderId, string sku, int quantity, decimal unitPrice)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sku))
        {
            errors.Add("Each item must include a SKU.");
        }

        if (quantity <= 0)
        {
            errors.Add("Each item quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            errors.Add("Each item unit price cannot be negative.");
        }

        if (errors.Count > 0)
        {
            throw new OrderValidationException(new Dictionary<string, string[]>
            {
                ["items"] = errors.ToArray()
            });
        }

        return new OrderItem(Guid.NewGuid(), orderId, sku.Trim(), quantity, unitPrice);
    }
}
