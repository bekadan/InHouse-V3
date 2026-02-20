namespace InHouse.BuildingBlocks.Messaging;

public sealed class OutboxPublisherOptions
{
    public int BatchSize { get; init; } = 50;
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);
    public int MaxRetryCount { get; init; } = 5;
}
