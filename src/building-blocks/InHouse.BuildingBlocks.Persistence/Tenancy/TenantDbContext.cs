using InHouse.BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Tenancy;

public abstract class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected TenantDbContext(DbContextOptions options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        // ITenantEntity implement eden tüm entity’lere global filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null) continue;

            if (!typeof(ITenantEntity).IsAssignableFrom(clrType))
                continue;

            // e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId
            var method = typeof(TenantDbContext)
                .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(clrType);

            method.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
    }
}
