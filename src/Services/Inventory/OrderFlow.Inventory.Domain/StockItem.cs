namespace OrderFlow.Inventory.Domain;

public sealed class StockItem
{
    private StockItem()
    {
    }

    public StockItem(string sku, int availableQuantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required.", nameof(sku));
        }

        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity));
        }

        Sku = sku.Trim();
        AvailableQuantity = availableQuantity;
    }

    public string Sku { get; private set; } = string.Empty;

    public int AvailableQuantity { get; private set; }

    public bool CanReserve(int quantity) => quantity > 0 && AvailableQuantity >= quantity;

    public void Reserve(int quantity)
    {
        if (!CanReserve(quantity))
        {
            throw new InvalidOperationException("Insufficient stock is available.");
        }

        AvailableQuantity -= quantity;
    }
}
