namespace InHouse.BuildingBlocks.Abstractions.Messaging;

public interface IIntegrationEvent
{
    string EventType { get; }
    DateTime OccurredOnUtc { get; }
}