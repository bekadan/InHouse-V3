using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Projection;

public interface IProjectionHandler
{
    string EventType { get; }
    int EventVersion { get; }

    Task ProjectAsync(IntegrationEventEnvelope envelope, CancellationToken ct);
}