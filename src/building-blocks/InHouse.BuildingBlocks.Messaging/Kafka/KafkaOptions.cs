namespace InHouse.BuildingBlocks.Messaging.Kafka;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; init; } = default!;
    public string Topic { get; init; } = "inhouse.integration.v1";
    public string ConsumerGroupId { get; init; } = "inhouse.default";
    public string DlqTopic { get; init; } = "inhouse.integration.v1.dlq";

    public int MaxPoisonAttempts { get; init; } = 10;
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(1);

    // Producer
    public bool EnableIdempotence { get; init; } = true;
    public int MessageTimeoutMs { get; init; } = 30000;

    // Consumer
    public bool EnableAutoCommit { get; init; } = false;
    public int SessionTimeoutMs { get; init; } = 10000;
    public int MaxPollIntervalMs { get; init; } = 300000;

    public int Concurrency { get; init; } = 1; // 1 = safest (ordering). Increase carefully.
}