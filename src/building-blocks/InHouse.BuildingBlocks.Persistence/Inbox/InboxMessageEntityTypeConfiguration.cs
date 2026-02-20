using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InHouse.BuildingBlocks.Persistence.Inbox;

public sealed class InboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> b)
    {
        b.ToTable("InboxMessages");

        b.HasKey(x => x.Id);

        b.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        b.Property(x => x.ConsumerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.MessageId).HasMaxLength(200).IsRequired();

        b.Property(x => x.ReceivedOnUtc).IsRequired();

        b.Property(x => x.LeaseId);
        b.Property(x => x.LeaseExpiresOnUtc);

        b.Property(x => x.ProcessedOnUtc);

        b.Property(x => x.AttemptCount).IsRequired();
        b.Property(x => x.LastAttemptOnUtc);
        b.Property(x => x.LastError).HasMaxLength(4000);

        // Hard guarantee: a message is processed at most once per (tenant, consumer)
        b.HasIndex(x => new { x.TenantId, x.ConsumerName, x.MessageId })
            .IsUnique();

        // Helpful for sweeps/monitoring
        b.HasIndex(x => new { x.ConsumerName, x.ProcessedOnUtc });
        b.HasIndex(x => new { x.LeaseExpiresOnUtc });
    }
}