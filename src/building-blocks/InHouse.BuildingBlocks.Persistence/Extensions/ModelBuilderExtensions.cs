using InHouse.BuildingBlocks.Persistence.Idempotency;
using InHouse.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        return modelBuilder;
    }

    public static ModelBuilder AddIdempotency(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IdempotencyRecordConfiguration());
        return modelBuilder;
    }
}
