namespace InHouse.BuildingBlocks.Persistence.Options;

public sealed class PersistenceOptions
{
    public int OutboxBatchSize { get; init; } = 100;

    public TimeSpan IdempotencyRetention { get; init; } = TimeSpan.FromDays(14);
}
