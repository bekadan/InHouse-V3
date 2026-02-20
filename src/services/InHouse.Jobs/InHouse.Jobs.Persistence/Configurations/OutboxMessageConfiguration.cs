using InHouse.Jobs.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InHouse.Jobs.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();

        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.ProcessedOnUtc);
        builder.Property(x => x.Error);

        builder.HasIndex(x => x.ProcessedOnUtc);
        builder.HasIndex(x => x.CreatedOnUtc);

        builder.Property(x => x.DeadLetteredOnUtc);
        builder.HasIndex(x => x.DeadLetteredOnUtc);

        builder.HasIndex(x => new { x.ProcessedOnUtc, x.ProcessingStartedOnUtc, x.CreatedOnUtc });
    }
}