using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.Payments.Infrastructure.Persistence;

internal sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("processed_messages");
        builder.HasKey(message => message.MessageId);
        builder.Property(message => message.ProcessedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
