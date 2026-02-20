using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes the specified integration event envelope to the message bus.
    /// The envelope must already contain all required transport metadata.
    /// </summary>
    Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}