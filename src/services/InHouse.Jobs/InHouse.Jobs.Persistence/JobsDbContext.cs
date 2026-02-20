using InHouse.BuildingBlocks.Persistence.SoftDelete;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using InHouse.Jobs.Domain.Entities;
using InHouse.Jobs.Persistence.Auditing;
using InHouse.Jobs.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace InHouse.Jobs.Persistence;

public sealed class JobsDbContext : DbContext
{
    private readonly ITenantProvider _tenant;
    private readonly ISoftDeleteFilterProvider _softDelete;

    public JobsDbContext(DbContextOptions<JobsDbContext> options, ITenantProvider tenant, ISoftDeleteFilterProvider softDelete)
        : base(options)
    {
        _tenant = tenant;
        _softDelete = softDelete;
    }
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>(builder =>
        {
            builder.ToTable("jobs");

            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(j => j.CompanyId)
                .IsRequired();

            builder.Property(j => j.CreatedOnUtc)
                .IsRequired();

            builder.HasIndex(j => j.CompanyId);
            builder.HasIndex(j => j.CreatedOnUtc);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsDbContext).Assembly);

        modelBuilder.ApplyCompanyTenantFilter(_tenant);

        modelBuilder.ApplySoftDeleteFilter(_softDelete);
    }
}