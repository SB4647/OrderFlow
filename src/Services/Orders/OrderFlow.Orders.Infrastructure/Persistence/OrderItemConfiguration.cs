using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Infrastructure.Persistence;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Sku)
            .HasMaxLength(maxLength: 64)
            .IsRequired();
        builder.Property(item => item.Quantity)
            .IsRequired();
        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
