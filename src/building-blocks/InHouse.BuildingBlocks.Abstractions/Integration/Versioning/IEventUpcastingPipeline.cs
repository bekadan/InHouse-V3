using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Versioning;

public interface IEventUpcastingPipeline
{
    /// <summary>
    /// Upcasts the envelope to the latest known version for its EventType.
    /// If no upcasters exist, returns the original envelope.
    /// </summary>
    IntegrationEventEnvelope UpcastToLatest(IntegrationEventEnvelope envelope);
}