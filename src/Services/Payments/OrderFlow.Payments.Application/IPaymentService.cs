using OrderFlow.Contracts;

namespace OrderFlow.Payments.Application;

public interface IPaymentService
{
    Task ProcessAsync(InventoryReserved inventoryReserved, CancellationToken cancellationToken);
}
