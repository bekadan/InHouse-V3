using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Bus;

public interface IMessageBusPublisher
{
    Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct);
}