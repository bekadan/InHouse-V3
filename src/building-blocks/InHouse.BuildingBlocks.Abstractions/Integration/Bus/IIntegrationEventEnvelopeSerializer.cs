using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Bus;

public interface IIntegrationEventEnvelopeSerializer
{
    byte[] Serialize(IntegrationEventEnvelope envelope);
    IntegrationEventEnvelope Deserialize(ReadOnlySpan<byte> bytes);
}