using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InHouse.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .IsRequired();

        builder.Property(x => x.HeadersJson);

        builder.Property(x => x.ProcessedOnUtc);
        builder.Property(x => x.Error);
        builder.Property(x => x.AttemptCount).IsRequired();

        builder.HasIndex(x => x.ProcessedOnUtc);
        builder.HasIndex(x => x.OccurredOnUtc);
    }
}
