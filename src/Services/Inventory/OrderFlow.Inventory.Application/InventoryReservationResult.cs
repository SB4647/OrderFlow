namespace OrderFlow.Inventory.Application;

public sealed record InventoryReservationResult(bool IsReserved, string? RejectionReason);
