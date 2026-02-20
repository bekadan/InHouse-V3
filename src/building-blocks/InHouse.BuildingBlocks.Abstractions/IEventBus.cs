namespace InHouse.BuildingBlocks.Abstractions;

public interface IEventBus
{
    Task PublishAsync(string eventType, string payloadJson, CancellationToken cancellationToken = default);
}
