using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Infrastructure.Persistence;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.HasIndex(order => order.CreatedByUserId);

        builder.Property(order => order.CreatedByUserId).IsRequired();
        builder.Property(order => order.CustomerName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(order => order.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
