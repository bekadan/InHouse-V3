using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Consumers;

public interface IIntegrationEventHandler
{
    /// <summary>Logical consumer name for idempotency uniqueness.</summary>
    string ConsumerName { get; }

    /// <summary>EventType this handler can process (e.g., "Jobs.JobPosted").</summary>
    string EventType { get; }

    /// <summary>EventVersion this handler expects (versioning strategy will extend this later).</summary>
    int EventVersion { get; }

    Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken ct);
}