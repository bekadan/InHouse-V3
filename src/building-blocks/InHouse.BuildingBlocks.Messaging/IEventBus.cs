namespace InHouse.BuildingBlocks.Messaging;

public interface IEventBus
{
    Task PublishAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default);
}
