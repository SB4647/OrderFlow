using OrderFlow.Contracts;

namespace OrderFlow.Inventory.Application;

public interface IInventoryService
{
    Task<InventoryReservationResult?> ReserveAsync(
        OrderSubmitted orderSubmitted,
        CancellationToken cancellationToken);
}
