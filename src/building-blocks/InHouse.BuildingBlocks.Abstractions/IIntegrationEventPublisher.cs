namespace InHouse.BuildingBlocks.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);
}