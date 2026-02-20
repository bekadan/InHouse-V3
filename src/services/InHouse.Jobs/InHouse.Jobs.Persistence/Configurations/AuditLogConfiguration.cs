using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InHouse.Jobs.Persistence.Auditing;

namespace InHouse.Jobs.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Resource).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ResourceId).HasMaxLength(128);

        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RequestId).HasMaxLength(128).IsRequired();

        builder.Property(x => x.ActorId).HasMaxLength(128);
        builder.Property(x => x.Ip).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        builder.Property(x => x.MetadataJson);

        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Success).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.OccurredOnUtc });

        // Idempotency-ish: aynı request içinde aynı log’u iki kez yazmayı engelle
        builder.HasIndex(x => new { x.RequestId, x.Action, x.Resource, x.ResourceId })
            .IsUnique();
    }
}