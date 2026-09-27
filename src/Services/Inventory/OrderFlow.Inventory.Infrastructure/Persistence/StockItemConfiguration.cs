using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(item => item.Sku);
        builder.Property(item => item.Sku)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(item => item.AvailableQuantity)
            .IsRequired();

        builder.HasData(
            new StockItem("KB-001", 10),
            new StockItem("MS-001", 5),
            new StockItem("MON-001", 3));
    }
}
