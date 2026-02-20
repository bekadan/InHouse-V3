using InHouse.BuildingBlocks.Abstractions.Messaging;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Events;

public interface IVersionedIntegrationEvent : IIntegrationEvent
{
    int Version { get; }
}