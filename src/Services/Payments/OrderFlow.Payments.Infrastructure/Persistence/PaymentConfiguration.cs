using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.Infrastructure.Persistence;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(payment => payment.Id);
        builder.HasIndex(payment => payment.OrderId).IsUnique();
        builder.Property(payment => payment.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(payment => payment.ProcessedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
