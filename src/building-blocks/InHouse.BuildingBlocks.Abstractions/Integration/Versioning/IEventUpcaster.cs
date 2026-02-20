using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Versioning;

/// <summary>
/// Upcasts an integration event envelope from one schema version to the next.
/// Upcasters should be pure (no IO). They must not depend on persistence.
/// </summary>
public interface IEventUpcaster
{
    string EventType { get; }

    /// <summary>The source version this upcaster accepts.</summary>
    int FromVersion { get; }

    /// <summary>The target version this upcaster produces. Typically FromVersion + 1.</summary>
    int ToVersion { get; }

    IntegrationEventEnvelope Upcast(IntegrationEventEnvelope envelope);
}