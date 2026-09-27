using OrderFlow.Contracts;

namespace OrderFlow.Inventory.Application;

public sealed class InventoryService(IInventoryRepository inventoryRepository) : IInventoryService
{
    public async Task<InventoryReservationResult?> ReserveAsync(
        OrderSubmitted orderSubmitted,
        CancellationToken cancellationToken)
    {
        if (await inventoryRepository.IsMessageProcessedAsync(orderSubmitted.MessageId, cancellationToken))
        {
            return null;
        }

        var requestedQuantities = orderSubmitted.Items
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity), StringComparer.Ordinal);

        InventoryReservationResult result;
        if (requestedQuantities.Count == 0 || requestedQuantities.Any(item => item.Value <= 0))
        {
            result = new InventoryReservationResult(false, "The order contains invalid items.");
        }
        else
        {
            var stockItems = await inventoryRepository.GetBySkusAsync(
                requestedQuantities.Keys.ToArray(),
                cancellationToken);
            var stockBySku = stockItems.ToDictionary(item => item.Sku, StringComparer.Ordinal);
            var unavailableSku = requestedQuantities.FirstOrDefault(item =>
                !stockBySku.TryGetValue(item.Key, out var stockItem) || !stockItem.CanReserve(item.Value));

            if (!string.IsNullOrEmpty(unavailableSku.Key))
            {
                result = new InventoryReservationResult(false, $"Insufficient stock for SKU '{unavailableSku.Key}'.");
            }
            else
            {
                foreach (var requestedQuantity in requestedQuantities)
                {
                    stockBySku[requestedQuantity.Key].Reserve(requestedQuantity.Value);
                }

                result = new InventoryReservationResult(true, null);
            }
        }

        inventoryRepository.MarkMessageProcessed(orderSubmitted.MessageId, DateTimeOffset.UtcNow);
        await inventoryRepository.SaveChangesAsync(cancellationToken);

        return result;
    }
}
