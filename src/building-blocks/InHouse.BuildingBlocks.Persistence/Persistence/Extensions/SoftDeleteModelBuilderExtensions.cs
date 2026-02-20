using InHouse.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.SoftDelete;

public static class SoftDeleteModelBuilderExtensions
{
    public static ModelBuilder ApplySoftDeleteFilter(
        this ModelBuilder modelBuilder,
        ISoftDeleteFilterProvider provider)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clr = entityType.ClrType;
            if (clr is null) continue;

            if (!typeof(ISoftDeletableEntity).IsAssignableFrom(clr))
                continue;

            var method = typeof(SoftDeleteModelBuilderExtensions)
                .GetMethod(nameof(SetFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(clr);

            method.Invoke(null, new object[] { modelBuilder, provider });
        }

        return modelBuilder;
    }

    private static void SetFilter<TEntity>(ModelBuilder modelBuilder, ISoftDeleteFilterProvider provider)
        where TEntity : class, ISoftDeletableEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => provider.BypassSoftDeleteFilter || !e.IsDeleted);
    }
}