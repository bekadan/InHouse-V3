using InHouse.BuildingBlocks.Persistence.SoftDelete;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using InHouse.Jobs.Domain;
using InHouse.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence;

public sealed class JobsReadDbContext : DbContext
{
    private readonly ITenantProvider _tenant;
    private readonly ISoftDeleteFilterProvider _softDelete;

    public JobsReadDbContext(DbContextOptions<JobsReadDbContext> options, ITenantProvider tenant, ISoftDeleteFilterProvider softDelete)
        : base(options)
    {   
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
        _tenant = tenant;
        _softDelete = softDelete;
    }

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>(builder =>
        {
            builder.ToTable("jobs");
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
            builder.Property(j => j.Status).IsRequired().HasMaxLength(50);
            builder.Property(j => j.CompanyId).IsRequired();
            builder.Property(j => j.CreatedOnUtc).IsRequired();

            builder.HasIndex(j => j.CompanyId);
            builder.HasIndex(j => j.CreatedOnUtc);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsReadDbContext).Assembly);

        modelBuilder.ApplySoftDeleteFilter(_softDelete);
    }

    public override int SaveChanges()
    => throw new InvalidOperationException("Read DbContext cannot save changes.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Read DbContext cannot save changes.");
}