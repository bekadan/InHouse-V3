using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace InHouse.BuildingBlocks.Persistence.Tenancy;

public static class TenantModelBuilderExtensions
{
    public static ModelBuilder ApplyCompanyTenantFilter(this ModelBuilder modelBuilder, ITenantProvider provider)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clr = entityType.ClrType;
            if (clr is null) continue;

            var companyIdProp = entityType.FindProperty("CompanyId");
            if (companyIdProp is null) continue;
            if (companyIdProp.ClrType != typeof(Guid)) continue;

            var method = typeof(TenantModelBuilderExtensions)
                .GetMethod(nameof(SetCompanyFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(clr);

            method.Invoke(null, new object[] { modelBuilder, provider });
        }

        return modelBuilder;
    }

    private static void SetCompanyFilter<TEntity>(ModelBuilder modelBuilder, ITenantProvider provider)
        where TEntity : class
    {
        // e => provider.BypassTenantFilter || (provider.TenantId != null && EF.Property<Guid>(e,"CompanyId") == provider.TenantId.Value)
        var param = Expression.Parameter(typeof(TEntity), "e");

        var bypass = Expression.Property(Expression.Constant(provider), nameof(ITenantProvider.BypassTenantFilter));

        var tenantIdExpr = Expression.Property(Expression.Constant(provider), nameof(ITenantProvider.TenantId));
        var tenantHasValue = Expression.Property(tenantIdExpr, nameof(Nullable<Guid>.HasValue));
        var tenantValue = Expression.Property(tenantIdExpr, nameof(Nullable<Guid>.Value));

        var companyId = Expression.Call(
            typeof(Microsoft.EntityFrameworkCore.EF),
            nameof(Microsoft.EntityFrameworkCore.EF.Property),
            new[] { typeof(Guid) },
            param,
            Expression.Constant("CompanyId"));

        var equal = Expression.Equal(companyId, tenantValue);
        var guarded = Expression.AndAlso(tenantHasValue, equal);

        var body = Expression.OrElse(bypass, guarded);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, param);

        modelBuilder.Entity<TEntity>().HasQueryFilter(lambda);
    }
}